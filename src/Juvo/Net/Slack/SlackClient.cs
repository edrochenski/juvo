// <copyright file="SlackClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Slack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

/*/ Delegates /*/

    /// <summary>
    /// Method that will handle the <see cref="SlackClient.MessageReceived"/> event.
    /// </summary>
    /// <param name="sender">Origin of the event.</param>
    /// <param name="arg">Argument containing data for the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public delegate Task MessageReceivedEventHandler(object sender, SlackMessage arg);

    /// <summary>
    /// Method that will handle the <see cref="SlackClient.PresenceChanged"/> event.
    /// </summary>
    /// <param name="sender">Origin of the event.</param>
    /// <param name="arg">Argument containing data for the event.</param>
    public delegate void PresenceChangedEventHandler(object sender, SlackPresenceChange arg);

    /// <summary>
    /// Method that will handle the <see cref="SlackClient.UserTyping"/> event.
    /// </summary>
    /// <param name="sender">Origin of the event.</param>
    /// <param name="arg">Argument containing data for the event.</param>
    public delegate void UserTypingEventHandler(object sender, SlackUserTyping arg);

    /// <summary>
    /// Slack client.
    /// </summary>
    public class SlackClient : ISlackClient
    {
/*/ Constants /*/
        private const string SlackParams = "simple_latest=1&no_unreads=1&pretty=1";
        private const string SlackUrl = "https://slack.com/api/";

/*/ Fields /*/
        private readonly StringContent emptyStringContent;
        private readonly ILog log;
        private readonly ILogManager logManager;
        private readonly IClientWebSocket webSocket;

        private string apiToken;
        private List<SlackChannel> channels;
        private string myId;
        private string myName;
        private List<SlackUser> users;
        private CancellationToken webSocketCancelToken;
        private string wsUrl;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackClient"/> class.
        /// </summary>
        /// <param name="clientWebSocket">Client web socket.</param>
        /// <param name="logManager">Log manager.</param>
        public SlackClient(IClientWebSocket clientWebSocket, ILogManager logManager)
        {
            this.webSocket = clientWebSocket ?? throw new ArgumentNullException(nameof(clientWebSocket));
            this.logManager = logManager;
            this.log = this.logManager?.GetLogger(typeof(SlackClient));

            this.emptyStringContent = new StringContent(string.Empty);
        }

        /*/ Events /*/

        /// <inheritdoc/>
        public event MessageReceivedEventHandler MessageReceived;

        /// <inheritdoc/>
        public event PresenceChangedEventHandler PresenceChanged;

        /// <inheritdoc/>
        public event UserTypingEventHandler UserTyping;

/*/ Methods /*/

    // Public

        /// <inheritdoc/>
        public async Task Connect()
        {
            var result = string.Empty;
            var postUrl = $"{SlackUrl}rtm.start?token={this.apiToken}&{SlackParams}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(SlackUrl);
                var response = await client.PostAsync(postUrl, this.emptyStringContent);
                result = await response.Content.ReadAsStringAsync();
            }

            var jObject = JObject.Parse(result);
            this.myId = jObject["self"]["id"].Value<string>();
            this.myName = jObject["self"]["name"].Value<string>();
            this.wsUrl = jObject["url"].Value<string>();

            var tempJson = jObject["channels"].ToString();
            this.channels = JsonConvert.DeserializeObject<List<SlackChannel>>(tempJson);

            tempJson = jObject["users"].ToString();
            this.users = JsonConvert.DeserializeObject<List<SlackUser>>(tempJson);

            this.webSocketCancelToken = new CancellationToken(false);
            await this.webSocket.ConnectAsync(new Uri(this.wsUrl), this.webSocketCancelToken);

            this.log?.Info($"Connected to slack as {this.myName}");

            this.Listen();
        }

        /// <inheritdoc/>
        public void Initialize(string token)
        {
            this.apiToken = token;
        }

        /// <inheritdoc/>
        public async Task SendMessage(string channel, string text)
        {
            var sendChannel = channel;

            if (channel.StartsWith("#") || channel.StartsWith("@"))
            {
                var chan = this.channels.FirstOrDefault(x => x.Name == channel.Remove(0, 1));
                if (chan.Equals(default(SlackChannel)))
                {
                    this.log?.Warn($"Could not resolve channel '{channel}'");
                    throw new Exception($"Invalid channel specified: {channel}");
                }

                sendChannel = chan.Id;
            }

            var result = string.Empty;
            var postUrl = string.Concat(
                SlackUrl,
                "chat.postMessage?token=",
                this.apiToken,
                "&as_user=true&mrkdwn=true",
                "&channel=",
                sendChannel,
                "&text=",
                this.SlackEncode(text));

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(SlackUrl);
                var response = await client.PostAsync(postUrl, this.emptyStringContent);
                result = await response.Content.ReadAsStringAsync();
            }
        }

    // Protected

        /// <summary>
        /// Called when <see cref="MessageReceived"/> is raised.
        /// </summary>
        /// <param name="arg">Data associated with the event.</param>
        protected virtual void OnMessageReceived(SlackMessage arg)
        {
            this.MessageReceived?.Invoke(this, arg);
            this.log?.Debug($"MSG: {arg.Text}");
        }

        /// <summary>
        /// Called when <see cref="PresenceChanged"/> is raised.
        /// </summary>
        /// <param name="arg">Data associated with the event.</param>
        protected virtual void OnPresenceChanged(SlackPresenceChange arg)
        {
            this.PresenceChanged?.Invoke(this, arg);
        }

        /// <summary>
        /// Called when <see cref="UserTyping"/> is raised.
        /// </summary>
        /// <param name="arg">Data associated with the event.</param>
        protected virtual void OnUserTyping(SlackUserTyping arg)
        {
            this.UserTyping?.Invoke(this, arg);
        }

    // Private
        private void HandleMessage(string message)
        {
            var jObject = JObject.Parse(message);

            if (jObject == null)
            {
                this.log?.Warn($"Could not parse message: '{message}'");
                return;
            }

            switch (jObject["type"].Value<string>().ToLowerInvariant())
            {
                case "message":
                {
                    var temp = JsonConvert.DeserializeObject<SlackMessage>(message);
                    this.OnMessageReceived(temp);
                    break;
                }

                case "presence_change":
                {
                    var temp = JsonConvert.DeserializeObject<SlackPresenceChange>(message);
                    this.OnPresenceChanged(temp);
                    break;
                }

                case "user_typing":
                {
                    var temp = JsonConvert.DeserializeObject<SlackUserTyping>(message);
                    this.OnUserTyping(temp);
                    break;
                }
            }
        }

        private async void Listen()
        {
            try
            {
                var buffer = new byte[4096];

                while (this.webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    var message = new StringBuilder();

                    do
                    {
                        if (this.webSocket.State != WebSocketState.Open)
                        {
                            this.log?.Warn($"WebSocket state changed during receive! (state: {this.webSocket.State})");
                            await this.Connect();
                            break;
                        }

                        result = await this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    if (message.Length > 0)
                    {
                        this.HandleMessage(message.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                this.log.Error($"Error while receiving: {exc.Message}");
                throw new Exception("Receiving data error", exc);
            }
        }

        private string SlackEncode(string message)
        {
            return message
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }
    }
}
