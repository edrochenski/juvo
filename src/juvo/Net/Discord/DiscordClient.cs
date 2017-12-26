// <copyright file="DiscordClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord
{
    using System;
    using System.Diagnostics;
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
    /// Function to handle the <see cref="DiscordClient.HelloResponseReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="response">Contents of event.</param>
    public delegate void HelloResponseReceivedEventHandler(object sender, HelloResponse response);

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.ReadyReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="data">Data associated with event.</param>
    public delegate void ReadyReceivedEventHandler(object sender, ReadyEventData data);

    /// <summary>
    /// Discord client.
    /// </summary>
    public class DiscordClient : IDisposable
    {
/*/ Constants /*/

        /// <summary>
        /// Default amount of time between heartbeats.
        /// </summary>
        public const int HeartbeatIntervalDefault = 60 * 1000;

        /// <summary>
        /// Receive indicator for incoming data.
        /// </summary>
        public const string RecInd = ">>";

        /// <summary>
        /// Send indicator for outgoing data.
        /// </summary>
        public const string SndInd = "<<";

/*/ Fields /*/
        private readonly CancellationToken cancelToken;
        private readonly Timer heartbeatTimer;
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
            this.heartbeatTimer = new Timer(
                this.OnHeartbeatInterval,
                null,
                HeartbeatIntervalDefault,
                HeartbeatIntervalDefault);
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

        /// <summary>
        /// Fires when a ready event is received.
        /// </summary>
        public event ReadyReceivedEventHandler ReadyReceived;

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

    // Protected

        /// <summary>
        /// Called when the heartbeat interval is reached.
        /// </summary>
        /// <param name="state">Stateful object assigned to the timer.</param>
        protected virtual void OnHeartbeatInterval(object state)
        {
            var d = this.lastSequence.HasValue ? this.lastSequence.Value.ToString() : "null";

            this.log.Info($"{SndInd} Heartbeat");
            this.SendText($"{{ \"op\": 1, \"d\": {d} }}").Wait();
        }

        /// <summary>
        /// Called when the hello response is received.
        /// </summary>
        /// <param name="response">Data associated with response.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnHelloResponseReceived(HelloResponse response)
        {
            this.log.Info($"{RecInd} Hello");
            this.lastSequence = response.Sequence;
            this.HeartbeatInterval = response.Data.HeartbeatInterval;

            this.SetHeartbeatInterval(this.HeartbeatInterval);

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

            // TODO: use an object and not literal json
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

            this.log.Info($"{SndInd} Identifying");
            await this.SendText(json);

            this.HelloResponseReceived?.Invoke(this, response);
        }

        /// <summary>
        /// Called when the Ready event is received.
        /// </summary>
        /// <param name="data">Data associated with the event.</param>
        protected virtual void OnReadyReceived(ReadyEventData data)
        {
            this.ReadyReceived?.Invoke(this, data);
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

        private async Task HandleMessage(string message)
        {
            var msg = JObject.Parse(message);
            switch ((int)msg["op"])
            {
                case 0: // Dispatch
                    switch (msg["t"].ToString())
                    {
                        case "READY":
                            this.OnReadyReceived(msg.ToObject<ReadyEventData>());
                            break;

                        default:
                            this.log.Warn($"Not capturing '{msg["t"]}' event.");
                            break;
                    }

                    break;

                case 1: // Heartbeat

                    break;

                case 9: // Invalid Session

                    break;

                case 10: // Hello
                    await this.OnHelloResponseReceived(msg.ToObject<HelloResponse>());
                    break;

                case 11: // Heartbeat ACK
                    this.log.Info($"{RecInd} Heartbeat ACK");
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
                    }
                    while (!result.EndOfMessage);

                    this.log.Debug($"{RecInd} {message}");

                    if (message.Length > 0)
                    {
                        await this.HandleMessage(message.ToString());
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

        private async Task SendText(string jsonString)
        {
            Debug.Assert(!string.IsNullOrEmpty(jsonString), $"{nameof(jsonString)} == null | empty");

            this.log.Debug($"{SndInd} {jsonString}");

            var encoded = Encoding.UTF8.GetBytes(jsonString);
            var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
            await this.socket.SendAsync(buffer, WebSocketMessageType.Text, true, this.cancelToken);
        }

        private void SetHeartbeatInterval(int intervalMs)
        {
            Debug.Assert(intervalMs > 0, $"{nameof(intervalMs)} <= 0");

            this.log.Info($"Heartbeat interval changed to {intervalMs}");
            this.heartbeatTimer.Change(intervalMs, intervalMs);
        }
    }
}
