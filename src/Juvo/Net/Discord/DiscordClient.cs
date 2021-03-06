﻿// <copyright file="DiscordClient.cs" company="https://gitlab.com/edrochenski/juvo">
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /*/ Delegates /*/

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.Disconnected"/> event.
    /// </summary>
    /// <param name="sender">Origin of the event.</param>
    /// <param name="arg">Contents of the event.</param>
    public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs arg);

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.GuildCreated"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="response">Contents of event.</param>
    public delegate void GuildCreatedEventHandler(object sender, GuildCreateResponse response);

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.HelloResponseReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="response">Contents of event.</param>
    public delegate void HelloResponseReceivedEventHandler(object sender, HelloResponse response);

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.MessageReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="response">Contents of event.</param>
    public delegate void MessageReceivedEventHandler(object sender, MessageCreateResponse response);

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.PresenceUpdated"/>  event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="response">Contents of event.</param>
    public delegate void PresenceUpdatedEventHandler(object sender, PresenceUpdateResponse response);

    /// <summary>
    /// Function to handle the <see cref="DiscordClient.ReadyReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of event.</param>
    /// <param name="data">Data associated with event.</param>
    public delegate void ReadyReceivedEventHandler(object sender, ReadyEventData data);

    /// <summary>
    /// Discord client.
    /// </summary>
    public class DiscordClient : IDiscordClient, IDisposable
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
        private readonly ILog? log;
        private readonly ILogManager? logManager;
        private readonly IClientWebSocket socket;
        private bool isConnected;
        private bool isDisposed;
        private int? lastSequence;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordClient"/> class.
        /// </summary>
        /// <param name="clientWebSocket">Client web socket.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="logManager">Log Manager.</param>
        public DiscordClient(
            IClientWebSocket clientWebSocket,
            IHttpClient httpClient,
            ILogManager? logManager = null)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.socket = clientWebSocket ?? throw new ArgumentNullException(nameof(clientWebSocket));
            this.logManager = logManager;

            this.cancelToken = new CancellationToken(false);
            this.heartbeatTimer = new Timer(
                this.OnHeartbeatInterval,
                null,
                HeartbeatIntervalDefault,
                HeartbeatIntervalDefault);
            this.log = this.logManager?.GetLogger(typeof(DiscordClient));
        }

        /*/ Events /*/

        /// <inheritdoc/>
        public event DisconnectedEventHandler? Disconnected;

        /// <summary>
        /// Fires when a GUILD_CREATE response is received.
        /// </summary>
        public event GuildCreatedEventHandler? GuildCreated;

        /// <summary>
        /// Fires when a hello response is received.
        /// </summary>
        public event HelloResponseReceivedEventHandler? HelloResponseReceived;

        /// <summary>
        /// Fires when a message is received.
        /// </summary>
        public event MessageReceivedEventHandler? MessageReceived;

        /// <summary>
        /// Fires when a presence updated response is received.
        /// </summary>
        public event PresenceUpdatedEventHandler? PresenceUpdated;

        /// <inheritdoc/>
        public event ReadyReceivedEventHandler? ReadyReceived;

        /*/ Properties /*/

        /// <summary>
        /// Gets the interval at which this client needs to send a heartbeat/ping.
        /// </summary>
        public int HeartbeatInterval { get; private set; } = Discord.DefaultHeartbeatInterval;

        /// <summary>
        /// Gets the Options for the discord client.
        /// </summary>
        public DiscordClientOptions? Options { get; private set; }

        /*/ Methods /*/

        /// <inheritdoc/>
        public async Task Connect()
        {
            if (this.Options is null) { throw new InvalidOperationException("Connect() called before Initialize()"); }

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

            if (this.socket.State != WebSocketState.Closed &&
                this.socket.State != WebSocketState.None)
            {
                var result = await this.CloseSocket();
                if (result != WebSocketState.Closed && result != WebSocketState.None)
                {
                    this.log?.Warn("Unable to close the web socket before reconnecting.");
                }
            }

            this.log?.Info($"Connecting to {wssUrl}");
            try
            {
                await this.socket.ConnectAsync(new Uri(wssUrl), this.cancelToken);
                this.isConnected = true;
                this.log?.Info("Connected to Discord, starting listener...");
                this.Listen();
            }
            catch (Exception exc)
            {
                this.log?.Error("Connect()", exc);
            }
        }

        /// <inheritdoc/>
        public async Task Disconnect()
        {
            await this.Disconnect(userInitiated: true);
        }

        /// <summary>
        /// Disposes of any resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual void Initialize(DiscordClientOptions options)
        {
            this.Options = options;
            if (this.Options.ApiUri == null)
            {
                this.Options.ApiUri = new Uri(Discord.DefaultApiUrl);
                this.Options.ApiVersion = Discord.DefaultApiVersion;
            }

            this.httpClient.BaseAddress =
                new Uri($"{this.Options.ApiUri}/v{this.Options.ApiVersion}/");

            var isBot = this.Options.IsBot ? "Bot " : string.Empty;
            var authToken = $"{isBot}{this.Options.AuthToken}";
            this.httpClient.DefaultRequestHeaders.Add("Authorization", authToken);
        }

        /// <summary>
        /// Dispose of any resources.
        /// </summary>
        /// <param name="isDisposing">Is dispose being called on the object.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!this.isDisposed && isDisposing)
            {
                this.socket?.Dispose();
                this.heartbeatTimer?.Dispose();
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Called when the client is disconnected.
        /// </summary>
        /// <param name="data">Data associated with the disconnection.</param>
        protected virtual void OnDisconnected(DisconnectedEventArgs data)
        {
            this.Disconnected?.Invoke(this, data);
        }

        /// <summary>
        /// Called when a new guild is created.
        /// </summary>
        /// <param name="response">Data associated with the response.</param>
        protected virtual void OnGuildCreated(GuildCreateResponse response)
        {
            Debug.Assert(response != null && response.Data != null, "Response data is null/missing");

            if (response is null || response.Data is null) { return; }

            this.log?.Info($"{RecInd} Created Guild: {response.Data.Name}");
            this.GuildCreated?.Invoke(this, response);
        }

        /// <summary>
        /// Called when the heartbeat interval is reached.
        /// </summary>
        /// <param name="state">Stateful object assigned to the timer.</param>
        protected virtual async void OnHeartbeatInterval(object? state)
        {
            if (this.isConnected)
            {
                var d = this.lastSequence.HasValue ? this.lastSequence.Value.ToString() : "null";

                this.log?.Info($"{SndInd} Heartbeat");
                await this.SendText($"{{ \"op\": 1, \"d\": {d} }}");
            }
        }

        /// <summary>
        /// Called when the hello response is received.
        /// </summary>
        /// <param name="response">Data associated with response.</param>
        /// <returns>Task.</returns>
        protected virtual async Task OnHelloResponseReceived(HelloResponse response)
        {
            Debug.Assert(response != null && response.Data != null, "Response data is null/missing");

            this.log?.Info($"{RecInd} Hello");
            this.lastSequence = response.Sequence;
            this.HeartbeatInterval = response.Data is null ? -1 : response.Data.HeartbeatInterval;

            this.SetHeartbeatInterval(this.HeartbeatInterval);

            var ident = new IdentityRequest
            {
                Data = new IdentityRequest.IdentityData()
                {
                    Compress = false,
                    LargeThreshold = 250,
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
                    Token = this.Options!.AuthToken
                },
                OpCode = GatewayOpCode.Identity,
                Shard = new int[0]
            };

            this.log?.Info($"{SndInd} Identifying");
            await this.SendText(JsonConvert.SerializeObject(ident));

            this.HelloResponseReceived?.Invoke(this, response);
        }

        /// <summary>
        /// Called when a message response is received.
        /// </summary>
        /// <param name="response">Data associated with the response.</param>
        protected virtual void OnMessageReceived(MessageCreateResponse response)
        {
            Debug.Assert(!(response is null || response.Data is null), "Response data is missing/null");
            if (response is null || response.Data is null) { return; }

            this.log?.Info(
                $"{RecInd} [{response.Data.GuildId}/{response.Data.ChannelId}/{response.Data.Author?.Id}]" +
                $"{response.Data.Content}");
            this.MessageReceived?.Invoke(this, response);
        }

        /// <summary>
        /// Called when the Presence Updated response is received.
        /// </summary>
        /// <param name="response">Data associated with the response.</param>
        protected virtual void OnPresenceUpdatedReceived(PresenceUpdateResponse response)
        {
            Debug.Assert(!(response is null || response.Data is null), "Response data is missing/null");
            if (response is null || response.Data is null) { return; }

            this.log?.Info(
                $"{RecInd} Presence: {response.Data.User?.Id} is now {response.Data.Status}");
            this.PresenceUpdated?.Invoke(this, response);
        }

        /// <summary>
        /// Called when the Ready event is received.
        /// </summary>
        /// <param name="data">Data associated with the event.</param>
        protected virtual void OnReadyReceived(ReadyEventData data)
        {
            Debug.Assert(!(data is null || data.Data is null), "Event data is missing/null");
            if (data is null || data.Data is null) { return; }

            this.log?.Info($"{RecInd} Ready -- Session '{data.Data.SessionId}'");
            this.ReadyReceived?.Invoke(this, data);
        }

        private string BuildWssUrl()
        {
            return $"{this.Options?.GatewayUri}/?v={this.Options?.ApiVersion}&" +
                   $"encoding={Discord.DefaultApiEncoding}";
        }

        private async Task<WebSocketState> CloseSocket()
        {
            if (this.IsValidOpenState(this.socket.State))
            {
                await this.socket.CloseOutputAsync(
                    WebSocketCloseStatus.Empty, string.Empty, this.cancelToken);
                await this.socket.CloseAsync(
                    WebSocketCloseStatus.EndpointUnavailable, string.Empty, this.cancelToken);
            }

            this.log?.Info("Connection closed!");
            return this.socket.State;
        }

        private async Task Disconnect(bool userInitiated)
        {
            try
            {
                this.isConnected = false;
                await this.CloseSocket();
                this.OnDisconnected(new DisconnectedEventArgs { UserInitiated = userInitiated });
            }
            catch (Exception exc)
            {
                this.log?.Error("in Disconnect()", exc);
            }
        }

        private async Task GetGateway()
        {
            if (this.Options is null) { throw new InvalidOperationException("GetGateway() called before Initialize()"); }

            this.log?.Debug(
                $"Retrieving gateway from " +
                $"{this.httpClient.BaseAddress}{Discord.ApiPaths.Gateway}");
            var response = await this.httpClient.GetStringAsync(Discord.ApiPaths.Gateway);
            this.Options.GatewayUri =
                new Uri(JsonConvert.DeserializeObject<GatewayResponse>(response)?.Url ?? string.Empty);
        }

        private async Task GetGatewayBot()
        {
            if (this.Options is null) { throw new InvalidOperationException("GetGatewayBot() called before Initialize()"); }

            this.log?.Debug(
                $"Retrieving bot-gateway from " +
                $"{this.httpClient.BaseAddress}{Discord.ApiPaths.GatewayBot}");
            var json = await this.httpClient.GetStringAsync(Discord.ApiPaths.GatewayBot);
            var resp = JsonConvert.DeserializeObject<GatewayBotResponse>(json);
            this.Options.GatewayUri = new Uri(resp.Url);
            this.Options.GatewayShards = resp.Shards;
        }

        private async Task HandleMessage(string message)
        {
            var msg = JObject.Parse(message);
            var msgOp = msg["op"];
            var msgT = msg["t"]?.ToString();

            if (msgOp is null || msg["t"] is null) { return; }
            switch ((int)msgOp)
            {
                case 0: // Dispatch
                {
                    switch (msgT)
                    {
                        case "GUILD_CREATE":
                        {
                            var response = msg.ToObject<GuildCreateResponse>();
                            if (response is GuildCreateResponse) { this.OnGuildCreated(response); }
                            break;
                        }

                        case "MESSAGE_CREATE":
                        {
                            var response = msg.ToObject<MessageCreateResponse>();
                            if (response is MessageCreateResponse) { this.OnMessageReceived(response); }
                            break;
                        }

                        case "PRESENCE_UPDATE":
                        {
                            var response = msg.ToObject<PresenceUpdateResponse>();
                            if (response is PresenceUpdateResponse) { this.OnPresenceUpdatedReceived(response); }
                            break;
                        }

                        case "READY":
                        {
                            var response = msg.ToObject<ReadyEventData>();
                            if (response is ReadyEventData) { this.OnReadyReceived(response); }
                            break;
                        }

                        default:
                        {
                            this.log?.Warn($"Not capturing '{msg["t"]}' event.");
                            break;
                        }
                    }

                    break;
                }

                case 1: // Heartbeat
                case 9: // Invalid Session
                {
                    break;
                }

                case 10: // Hello
                {
                    var response = msg.ToObject<HelloResponse>();
                    if (response is HelloResponse) { await this.OnHelloResponseReceived(response); }
                    break;
                }

                case 11: // Heartbeat ACK
                {
                    this.log?.Info($"{RecInd} Heartbeat ACK");
                    break;
                }

                default:
                {
                    this.log?.Warn(msg.ToString().Replace("\n", string.Empty).Replace("\r", string.Empty));
                    break;
                }
            }
        }

        private bool IsValidOpenState(WebSocketState state)
        {
            return state == WebSocketState.Open || state == WebSocketState.CloseReceived;
        }

        private async void Listen()
        {
            try
            {
                var buffer = new byte[4096];

                while (this.isConnected && this.socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    var message = new StringBuilder();

                    do
                    {
                        result = await this.socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None);
                        message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    this.log?.Debug($"{RecInd} {message}");

                    if (message.Length > 0)
                    {
                        await this.HandleMessage(message.ToString());
                    }
                    else
                    {
                        if (this.socket.State == WebSocketState.CloseReceived)
                        {
                            this.log?.Debug("Close request received");
                            await this.Disconnect(userInitiated: false);
                        }
                    }
                }
            }
            catch (WebSocketException)
            {
                await this.Disconnect(false);
            }
            catch (TaskCanceledException)
            {
                this.log?.Debug("Listen(): TaskCanceledException suppressed.");
                await this.Disconnect(false);
            }
            catch (Exception exc)
            {
                this.log?.Error("Listen()", exc);
                await this.Disconnect(false);
            }
        }

        private async Task SendText(string jsonString)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(jsonString),
                $"{nameof(jsonString)} == null | empty");

            this.log?.Debug($"{SndInd} {jsonString}");

            if (this.socket.State == WebSocketState.Open)
            {
                try
                {
                    var encoded = Encoding.UTF8.GetBytes(jsonString);
                    var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                    await this.socket.SendAsync(
                        buffer,
                        WebSocketMessageType.Text,
                        true,
                        this.cancelToken);
                }
                catch (Exception exc)
                {
                    this.log?.Error("SendText()", exc);
                }
            }
            else
            {
                this.log?.Warn($"Socket in '{this.socket.State}' state. Could not send message.");
            }
        }

        private void SetHeartbeatInterval(int intervalMs)
        {
            Debug.Assert(intervalMs > 0, $"{nameof(intervalMs)} <= 0");

            this.log?.Info($"Heartbeat interval changed to {intervalMs}");
            this.heartbeatTimer.Change(intervalMs, intervalMs);
        }
    }
}
