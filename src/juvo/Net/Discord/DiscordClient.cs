// <copyright file="DiscordClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord
{
    using System;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Net.Discord.Model;
    using log4net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

/*/ Delegates /*/

    /// <summary>
    /// Function to hand the <see cref="DiscordClient.HelloResponseReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="response">Contents of event.</param>
    public delegate void HelloResponseReceivedEventHandler(object sender, HelloResponse response);

    /// <summary>
    /// Discord client.
    /// </summary>
    public class DiscordClient : IDisposable
    {
/*/ Fields /*/
        private readonly CancellationToken cancelToken;
        private readonly IHttpClient httpClient;
        private readonly ILog log;
        private readonly IClientWebSocket socket;
        private int? lastSequence;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordClient"/> class.
        /// </summary>
        /// <param name="clientWebSocket">Client web socket to use.</param>
        /// <param name="httpClient">Http client to use.</param>
        /// <param name="options">Options for the discord client.</param>
        public DiscordClient(
            IClientWebSocket clientWebSocket,
            IHttpClient httpClient,
            DiscordClientOptions options = null)
        {
            this.cancelToken = new CancellationToken(false);
            this.httpClient = httpClient;
            this.log = LogManager.GetLogger(typeof(DiscordClient));
            this.socket = clientWebSocket;
            this.Options = options ?? new DiscordClientOptions();

            if (this.Options.ApiUri == null)
            {
                this.Options.ApiUri = new Uri(Discord.DefaultApiUrl);
                this.Options.ApiVersion = Discord.DefaultApiVersion;
            }

            this.httpClient.BaseAddress =
                new Uri($"{this.Options.ApiUri}/v{this.Options.ApiVersion}/");

            var authToken = string.Format(
                "{0}{1}", this.Options.IsBot ? "Bot " : string.Empty, this.Options.AuthToken);
            this.httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
        }

/*/ Events /*/

        /// <summary>
        /// Fires when a hello response is received.
        /// </summary>
        public event HelloResponseReceivedEventHandler HelloResponseReceived;

/*/ Properties /*/

    // Public

        /// <summary>
        /// Gets the interval at which this client needs to send a heartbeat/ping.
        /// </summary>
        public int HeartbeatInterval { get; private set; } = Discord.DefaultHeartbeatInterval;

        /// <summary>
        /// Gets the Options for the discord client.
        /// </summary>
        public DiscordClientOptions Options { get; private set; }

/*/ Methods /*/

    // Public

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        /// <returns>A Task associated with the async operation.</returns>
        public async Task Connect()
        {
            if (this.Options.GatewayUri == null)
            {
                if (this.Options.IsBot)
                {
                    await this.GetGatewayBot();
                }
                else
                {
                    await this.GetGateway();
                }
            }

            var wssUrl = this.BuildWssUrl();

            this.log.Info($"Connecting to {wssUrl}");
            await this.socket.ConnectAsync(new Uri(wssUrl), this.cancelToken);

            this.log.Info("Connected to Discord, starting listener...");
            this.Listen();
        }

        /// <summary>
        /// Disposes of any resources.
        /// </summary>
        public void Dispose()
        {
            // TODO: properly implement dispose() and dispose(bool)
            this.socket?.Dispose();
        }

    // Private
    // |
        private string BuildWssUrl()
        {
            return $"{this.Options.GatewayUri}/?v={this.Options.ApiVersion}&" +
                   $"encoding={Discord.DefaultApiEncoding}";
        }

        private async Task GetGateway()
        {
            this.log.Debug(
                $"Retrieving gateway from {this.httpClient.BaseAddress}{Discord.ApiPaths.Gateway}");
            var response = await this.httpClient.GetStringAsync(Discord.ApiPaths.Gateway);
            this.Options.GatewayUri = new Uri(JsonConvert.DeserializeObject<GatewayResponse>(response)?.Url);
        }

        private async Task GetGatewayBot()
        {
            this.log.Debug(
                $"Retrieving bot-gateway from {this.httpClient.BaseAddress}{Discord.ApiPaths.GatewayBot}");
            var json = await this.httpClient.GetStringAsync(Discord.ApiPaths.GatewayBot);
            var resp = JsonConvert.DeserializeObject<GatewayBotResponse>(json);
            this.Options.GatewayUri = new Uri(resp.Url);
            this.Options.GatewayShards = resp.Shards;
        }

        private void HandleMessage(string message)
        {
            var msg = JObject.Parse(message);
            switch ((int)msg["op"])
            {
                case 10:
                    this.OnHelloResponseReceived(msg.ToObject<HelloResponse>());
                    break;
                default:
                    this.log.Warn(
                        msg.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty));
                    break;
            }
        }

        private async void Listen()
        {
            try
            {
                var buffer = new byte[4096];

                while (this.socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    var message = new StringBuilder();

                    do
                    {
                        result = await this.socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        this.log.Debug("Building message...");
                    }
                    while (!result.EndOfMessage);

                    this.log.Debug($"Msg: {message.Length}: {message}");

                    if (message.Length > 0)
                    {
                        this.HandleMessage(message.ToString());
                    }
                    else
                    {
                        this.log.Info("Connection closed!");
                    }
                }
            }
            catch (Exception exc)
            {
                this.log.Error(exc.Message);
                throw;
            }
        }

        private void OnHelloResponseReceived(HelloResponse response)
        {
            this.lastSequence = response.Sequence;
            this.HeartbeatInterval = response.Data.HeartbeatInterval;

            var ident = new IdentityRequest
            {
                Compress = true,
                LargeThreshold = 250,
                OpCode = GatewayOpCode.Identity,
                Presence = new IdentityRequest.StatusUpdate
                {
                    Afk = false,
                    Game = null,
                    Since = null,
                    Status = "online"
                },
                Properties = new IdentityRequest.ConnectionProperties
                {
                    Browser = "disco",
                    Device = "disco",
                    Os = "windows" // Environment.OSVersion.Platform.ToString()
                },
                Token = this.Options.AuthToken
            };
            var json = "{" // JsonConvert.SerializeObject(ident);
                        + "\"op\": 2,"
                        + "\"d\":{"
                            + "\"compress\": false,"
                            + "\"large_threshold\": 250,"
                            + "\"presence\": {"
                                + "\"afk\": false,"
                                + "\"game\": null,"
                                + "\"since\": null,"
                                + "\"status\": \"online\""
                            + "},"
                            + "\"properties\": {"
                                + "\"$browser\": \"disco\","
                                + "\"$device\": \"disco\","
                                + "\"$os\": \"windows\""
                            + "},"
                            + "\"token\": \"Mzc2NTM1NDQ0OTIzMzUxMDQx.DOaZWw.bYZO4nsYktA83HCeK1zneFNKtfY\""
                        + "}"

                    // + "\"shard\": [],"
                    // + "\"t\": null,"
                    // + "\"s\": null,"
                    + "}";

            var encoded = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);

            this.log.Info("Identifying...");
            this.log.Debug($"Using: {json}");
            this.socket.SendAsync(buffer, WebSocketMessageType.Text, true, this.cancelToken).Wait();

            this.HelloResponseReceived?.Invoke(this, response);
        }
    }
}
