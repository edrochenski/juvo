// <copyright file="IrcClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// IRC client.
    /// </summary>
    public class IrcClient : IDisposable, IIrcClient
    {
/*/ Constants /*/

    // Public

        /// <summary>
        /// Characters that channels can begin with.
        /// </summary>
        public const string ChannelIdents = "&#+!";

        /// <summary>
        /// Line ending for IRC commands (\r\n)
        /// </summary>
        public const string CrLf = "\r\n";

    // Private
        private const int BufferSize = 4096;
        private const int DefaultPort = 6667;

/*/ Fields /*/
        private static readonly Dictionary<string, IrcNetwork> IrcNetworkLookup;
        private readonly Dictionary<char, Tuple<IrcChannelMode, bool, bool>> chanModeDict;
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly Dictionary<char, IrcUserMode> userModeDict;

        private SocketClient client;
        private List<string> currentChannels;
        private string currentNickname;
        private StringBuilder dataBuffer;
        private string serverHost;
        private int serverPort;

/*/ Constructors /*/
        static IrcClient()
        {
            IrcNetworkLookup = new Dictionary<string, IrcNetwork>
            {
                { "undernet", IrcNetwork.Undernet }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        /// <param name="loggerFactory">LoggerFactory to create log.</param>
        public IrcClient(ILoggerFactory loggerFactory = null)
            : this(IrcNetwork.Unknown, loggerFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        /// <param name="network">Network to connect to.</param>
        /// <param name="loggerFactory">LoggerFactory to create log.</param>
        public IrcClient(IrcNetwork network, ILoggerFactory loggerFactory = null)
        {
            this.client = new SocketClient();
            this.client.ConnectCompleted += this.Client_ConnectCompleted;
            this.client.ConnectFailed += this.Client_ConnectFailed;
            this.client.Disconnected += this.Client_Disconnected;
            this.client.ReceiveCompleted += this.Client_ReceiveCompleted;
            this.client.ReceiveFailed += this.Client_ReceiveFailed;
            this.client.SendCompleted += this.Client_SendCompleted;
            this.client.SendFailed += this.Client_SendFailed;

            this.dataBuffer = new StringBuilder();
            this.currentChannels = new List<string>(0);

            this.Network = network;

            this.loggerFactory = loggerFactory;
            if (this.loggerFactory != null)
            {
                this.logger = this.loggerFactory.CreateLogger<IrcClient>();
            }

            this.chanModeDict = this.CompileChannelModeDictionary();
            this.userModeDict = this.CompileUserModeDictionary();
        }

    /*/ Events /*/

        /// <inheritdoc/>
        public event EventHandler<ChannelUserEventArgs> ChannelJoined;

        /// <inheritdoc/>
        public event EventHandler<ChannelUserEventArgs> ChannelMessage;

        /// <inheritdoc/>
        public event EventHandler<ChannelModeChangedEventArgs> ChannelModeChanged;

        /// <inheritdoc/>
        public event EventHandler<ChannelUserEventArgs> ChannelParted;

        /// <inheritdoc/>
        public event EventHandler Connected;

        /// <inheritdoc/>
        public event EventHandler Disconnected;

        /// <inheritdoc/>
        public event EventHandler<HostHiddenEventArgs> HostHidden;

        /// <inheritdoc/>
        public event EventHandler<MessageReceivedArgs> MessageReceived;

        /// <inheritdoc/>
        public event EventHandler<UserEventArgs> PrivateMessage;

        /// <inheritdoc/>
        public event EventHandler<IrcReply> ReplyReceived;

        /// <inheritdoc/>
        public event EventHandler<UserModeChangedEventArgs> UserModeChanged;

        /// <inheritdoc/>
        public event EventHandler<UserEventArgs> UserQuit;

/*/ Properties /*/

        /// <inheritdoc/>
        public List<string> CurrentChannels { get; protected set; }

        /// <inheritdoc/>
        public string CurrentNickname { get; protected set; }

        /// <inheritdoc/>
        public string NickName { get; set; }

        /// <inheritdoc/>
        public string NickNameAlt { get; set; }

        /// <inheritdoc/>
        public IrcNetwork Network { get; protected set; }

        /// <inheritdoc/>
        public string RealName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IrcUserMode> UserModes { get; protected set; }

        /// <inheritdoc/>
        public string Username { get; set; }

/*/ Methods /*/

    // Public

        /// <summary>
        /// Looks up IRC network.
        /// </summary>
        /// <param name="network">Name of network.</param>
        /// <returns>Resolved IRC network.</returns>
        public static IrcNetwork LookupNetwork(string network)
        {
            network = string.IsNullOrEmpty(network) ? string.Empty : network.ToLowerInvariant();
            return
                IrcNetworkLookup.ContainsKey(network)
                ? IrcNetworkLookup[network]
                : IrcNetwork.Unknown;
        }

        /// <inheritdoc/>
        public void Connect(string serverHost, int serverPort = DefaultPort)
        {
            Debug.Assert(!string.IsNullOrEmpty(serverHost), "serverHost == null||empty");
            Debug.Assert(serverPort > 1024, "serverPort <= 1024");

            this.serverHost = serverHost;
            this.serverPort = serverPort;

            this.logger?.LogInformation($"Attempting to connect to {serverHost} on port {serverPort}");

            this.client.Connect(this.serverHost, this.serverPort);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(true);
        }

        /// <inheritdoc/>
        public void Join(string channel) => this.Join(channel, string.Empty);

        /// <inheritdoc/>
        public void Join(string channel, string key = "")
        {
            Debug.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");
            this.Send($"JOIN {channel}{CrLf}");
        }

        /// <inheritdoc/>
        public void Join(string[] channels, string[] channelKeys = null)
        {
            Debug.Assert(channels != null && channels.Length > 0, "channels == null || empty");

            var chans = string.Join(",", channels);
            var keys = (channelKeys != null) ? " " + string.Join(",", channelKeys) : string.Empty;

            this.Send($"JOIN {chans}{keys}{CrLf}");
        }

        /// <summary>
        /// Looks up a channel mode.
        /// </summary>
        /// <param name="mode">Resolved mode.</param>
        /// <returns>A tuple with the channel mode, has add parameter, and has remove parameter.</returns>
        public(IrcChannelMode Mode, bool HasAddParam, bool HasRemParam) LookupChannelMode(char mode)
        {
            return (
                this.chanModeDict[mode].Item1,
                this.chanModeDict[mode].Item2,
                this.chanModeDict[mode].Item3);
        }

        /// <inheritdoc/>
        public IrcUserMode LookupUserMode(char mode)
        {
            return this.userModeDict[mode];
        }

        /// <inheritdoc/>
        public IEnumerable<IrcUserMode> LookupUserModes(char[] mode)
        {
            var result = new IrcUserMode[mode.Length];
            for (var x = 0; x < mode.Length; ++x)
            {
                result[x] = this.userModeDict[mode[x]];
            }

            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<IrcUserMode> LookupUserModes(string mode)
        {
            return this.LookupUserModes(mode.ToCharArray());
        }

        /// <inheritdoc/>
        public void Part(string channel, string message = "")
        {
            Debug.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");

            var msg = string.IsNullOrEmpty(message) ? string.Empty : string.Concat(" ", message);
            this.Send($"PART {channel}{msg}{CrLf}");
        }

        /// <inheritdoc/>
        public void Quit(string message = "")
        {
            this.Send($"QUIT :{message}{CrLf}");
        }

        /// <inheritdoc/>
        public void Send(string data)
        {
            this.client.Send(UTF8Encoding.UTF8.GetBytes(data));
        }

        /// <inheritdoc/>
        public void Send(string format, params object[] args)
        {
            this.client.Send(string.Format(format, args));
        }

        /// <inheritdoc/>
        public void Send(byte[] data)
        {
            this.client.Send(data);
        }

        /// <inheritdoc/>
        public void SendMessage(string to, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            this.Send($"PRIVMSG {to} :{msg}{CrLf}");
        }

    // Protected

        /// <summary>
        /// Dispose of the instance's resources.
        /// </summary>
        /// <param name="disposing">Should managed resources be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.client?.Dispose();
            }
        }

        /// <summary>
        /// Called when the <see cref="ChannelJoined"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnChannelJoined(ChannelUserEventArgs e)
        {
            this.ChannelJoined?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="ChannelMessage"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnChannelMessage(ChannelUserEventArgs e)
        {
            this.ChannelMessage?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="ChannelModeChanged"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnChannelModeChanged(ChannelModeChangedEventArgs e)
        {
            this.ChannelModeChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="ChannelParted"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnChannelParted(ChannelUserEventArgs e)
        {
            this.ChannelParted?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="Connected"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnConnected(EventArgs e)
        {
            this.currentNickname = this.NickName; // TODO: don't assume the first nick worked
            this.Connected?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the DataReceived event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnDataReceived(DataReceivedArgs e)
        {
        }

        /// <summary>
        /// Called when the <see cref="Disconnected"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnDisconnected(EventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="HostHidden"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnHostHidden(HostHiddenEventArgs e)
        {
            this.HostHidden?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="MessageReceived"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnMessageReceived(MessageReceivedArgs e)
        {
            this.MessageReceived?.Invoke(this, e);
            this.HandleMessage(e.Message);
        }

        /// <summary>
        /// Called when the <see cref="PrivateMessage"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnPrivateMessage(UserEventArgs e)
        {
            this.PrivateMessage?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="ReplyReceived"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnReplyReceived(IrcReply e)
        {
            this.ReplyReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="UserModeChanged"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnUserModeChanged(UserModeChangedEventArgs e)
        {
            this.UserModeChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Called when the <see cref="UserQuit"/> event is raise.
        /// </summary>
        /// <param name="e">Data associated with the event.</param>
        protected virtual void OnUserQuit(UserEventArgs e)
        {
            this.UserQuit?.Invoke(this, e);
        }

    // Private
        private void Client_ConnectCompleted(object sender, EventArgs e)
        {
            this.Send($"NICK {this.NickName}\r\nUSER {this.Username} 0 * :{this.Username}\r\n");
        }

        private void Client_ConnectFailed(object sender, EventArgs e)
        {
            this.logger?.LogError("Connection failed");
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
        }

        private void Client_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (this.client == null)
            {
                return;
            }

            this.OnDataReceived(new DataReceivedArgs(e.Data));
            this.HandleData(e.Data);
        }

        private void Client_ReceiveFailed(object sender, SocketEventArgs e)
        {
            this.logger?.LogError($"Receive failed ({e.Error})");
        }

        private void Client_SendCompleted(object sender, EventArgs e)
        {
        }

        private void Client_SendFailed(object sender, SocketEventArgs e)
        {
            this.logger?.LogError($"Send failed ({e.Error})");
        }

        private Dictionary<char, Tuple<IrcChannelMode, bool, bool>> CompileChannelModeDictionary()
        {
            var result = new Dictionary<char, Tuple<IrcChannelMode, bool, bool>>
            {
                { 'b', Tuple.Create(IrcChannelMode.Ban, true, true) },
                { 'e', Tuple.Create(IrcChannelMode.Exception, true, true) },
                { 'i', Tuple.Create(IrcChannelMode.InviteOnly, false, false) },
                { 'k', Tuple.Create(IrcChannelMode.Key, true, true) },
                { 'l', Tuple.Create(IrcChannelMode.Limit, true, false) },
                { 'm', Tuple.Create(IrcChannelMode.Moderated, false, false) },
                { 'n', Tuple.Create(IrcChannelMode.NoExternal, false, false) },
                { 'o', Tuple.Create(IrcChannelMode.Operator, true, true) },
                { 'p', Tuple.Create(IrcChannelMode.Private, false, false) },
                { 's', Tuple.Create(IrcChannelMode.Secret, false, false) },
                { 't', Tuple.Create(IrcChannelMode.TopicLock, false, false) },
                { 'v', Tuple.Create(IrcChannelMode.Voiced, true, true) }
            };

            if (this.Network == IrcNetwork.Undernet)
            {
                result.Add('D', Tuple.Create(IrcChannelMode.DelayJoin, false, false));
                result.Add('R', Tuple.Create(IrcChannelMode.Registered, false, false));
                result.Add('r', Tuple.Create(IrcChannelMode.RegisterForJoin, false, false));
            }

            return result;
        }

        private Dictionary<char, IrcUserMode> CompileUserModeDictionary()
        {
            var result = new Dictionary<char, IrcUserMode>
            {
                { 'a', IrcUserMode.Away },
                { 'd', IrcUserMode.Deaf },
                { 'i', IrcUserMode.Invisible },
                { 'o', IrcUserMode.Operator },
                { 'r', IrcUserMode.Restricted },
                { 's', IrcUserMode.ServerNotices },
                { 'w', IrcUserMode.Wallops },
                { '0', IrcUserMode.OperatorLocal }
            };

            if (this.Network == IrcNetwork.Undernet)
            {
                result.Add('x', IrcUserMode.HiddenHost);
            }

            return result;
        }

        private void HandleData(byte[] data)
        {
            string incoming = UTF8Encoding.UTF8.GetString(data);
            this.dataBuffer.Append(incoming);

            while (this.dataBuffer.ToString().Contains("\r\n"))
            {
                string temp = this.dataBuffer.ToString();
                int rnIndex = temp.IndexOf("\r\n");
                int length = temp.Length - (temp.Length - rnIndex);

                string message = temp.Substring(0, length);
                this.OnMessageReceived(new MessageReceivedArgs(message));

                this.dataBuffer.Remove(0, length + 2);
            }
        }

        private void HandleMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (message.StartsWith(":"))
            {
                this.HandleReply(message);
                return;
            }

            string[] msgParts = message.Split(' ');
            switch (msgParts[0].Trim().ToUpperInvariant())
            {
                case "NOTICE":
                    break;
                case "PING":
                    string pingSource = msgParts[1].Replace(":", string.Empty);
                    this.Send("PONG {0}\r\n", pingSource);
                    break;
            }
        }

        private void HandleReply(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var reply = new IrcReply(message);
            var cmd = reply.Command.ToUpperInvariant();
            this.OnReplyReceived(reply);

            switch (cmd)
            {
                case "001":
                {
                    this.OnConnected(EventArgs.Empty);
                    break;
                }

                case "396":
                {
                    // :<server> 396 enloco enloco.users.undernet.org :is now your hidden host
                    this.OnHostHidden(new HostHiddenEventArgs(reply.Params[0]));
                    break;
                }

                case "JOIN":
                {
                    // :<nick>!<user>@<host> JOIN <channel>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = false;

                    if (user.Nickname == this.currentNickname)
                    {
                        this.currentChannels.Add(reply.Target);
                        isOwned = true;
                    }

                    this.OnChannelJoined(new ChannelUserEventArgs(reply.Target, user, isOwned));
                    break;
                }

                case "MODE":
                {
                    // USER MODE -> :n!u@h MODE <target> :[+]modes[-]modes
                    if (reply.TargetIsUser)
                    {
                        var add = string.Empty;
                        var rem = string.Empty;
                        var plus = true;
                        for (var x = 0; x < reply.Trailing.Length; ++x)
                        {
                            var ch = reply.Trailing[x];
                            if (ch == '+')
                            {
                                plus = true;
                            }
                            else if (ch == '-')
                            {
                                plus = false;
                            }
                            else if (plus)
                            {
                                add += ch;
                            }
                            else if (!plus)
                            {
                                rem += ch;
                            }
                        }

                        this.OnUserModeChanged(
                            new UserModeChangedEventArgs(
                                this.LookupUserModes(add),
                                this.LookupUserModes(rem)));
                    }
                    else if (reply.TargetIsChannel)
                    {
                        var add = new List<IrcChannelModeValue>(0);
                        var rem = new List<IrcChannelModeValue>(0);
                        var ops = reply.Params[0];
                        var ctr = '\0';

                        for (int x = 0, c = 0; x < ops.Length; ++x)
                        {
                            if ("-+".Contains(ops[x]))
                            {
                                ctr = ops[x];
                                continue;
                            }

                            var mode = this.LookupChannelMode(ops[x]);

                            if (ctr == '+')
                            {
                                var val = mode.HasAddParam ? reply.Params[++c] : null;
                                add.Add(new IrcChannelModeValue(mode.Mode, val));
                            }
                            else if (ctr == '-')
                            {
                                var val = mode.HasRemParam ? reply.Params[++c] : null;
                                rem.Add(new IrcChannelModeValue(mode.Mode, val));
                            }
                        }

                        this.OnChannelModeChanged(new ChannelModeChangedEventArgs(reply.Target, add, rem));
                    }

                    break;
                }

                case "PART":
                {
                    // :<nick>!<user>@<host> PART <channel> :<message>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = false;

                    if (user.Nickname == this.currentNickname)
                    {
                        this.currentChannels.Remove(reply.Target);
                        isOwned = true;
                    }

                    this.OnChannelParted(new ChannelUserEventArgs(reply.Target, user, isOwned, reply.Trailing, IrcMessageType.None));
                    break;
                }

                case "PRIVMSG":
                case "NOTICE":
                {
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = user.Nickname == this.currentNickname;
                    var msgType = (cmd == "PRIVMSG") ? IrcMessageType.PrivateMessage : IrcMessageType.Notice;

                    if (reply.TargetIsChannel)
                    {
                        this.OnChannelMessage(new ChannelUserEventArgs(
                            reply.Target, user, isOwned, reply.Trailing, msgType));
                    }
                    else
                    {
                        this.OnPrivateMessage(new UserEventArgs(
                            user, reply.Trailing, isOwned, msgType));
                    }

                    break;
                }

                case "QUIT":
                {
                    // :<nick>!<user>@<host> QUIT :<message>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = user.Nickname == this.currentNickname;

                    this.OnUserQuit(new UserEventArgs(user, reply.Trailing, isOwned));
                    break;
                }
            }
        }
    }
}
