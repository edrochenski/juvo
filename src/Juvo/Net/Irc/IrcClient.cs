// <copyright file="IrcClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Dbg = System.Diagnostics.Debug;

    /// <summary>
    /// IRC client.
    /// </summary>
    public class IrcClient : IDisposable, IIrcClient
    {
        /*/ Constants /*/

        /// <summary>
        /// Characters that channels can begin with.
        /// </summary>
        public const string ChannelIdents = "&#+!";

        /// <summary>
        /// Line ending for IRC commands (\r\n)
        /// </summary>
        public const string CrLf = "\r\n";

        /// <summary>
        /// Size limit of a single message in IRC.
        /// </summary>
        public const int MessageSizeLimit = 484;

        private const int BufferSize = 4096;
        private const int DefaultPort = 6667;
        private const string DefaultCommandToken = ".";
        private const int Debug = 0;
        private const int Info = 1;
        private const int Warn = 2;
        private const int Error = 3;
        private const int Fatal = 4;

        /*/ Fields /*/

        private static readonly Dictionary<string, IrcNetwork> IrcNetworkLookup;
        private readonly Dictionary<char, Tuple<IrcChannelMode, bool, bool>> chanModeDict;
        private readonly ISocketClient client;
        private readonly ILog? log;
        private readonly ILogManager? logManager;
        private readonly Dictionary<char, IrcUserMode> userModeDict;

        private StringBuilder dataBuffer;
        private string? serverHost;
        private string? serverPassword;
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
        /// <param name="logManager">Log manager.</param>
        /// <param name="socketClient">Socket client.</param>
        public IrcClient(ISocketClient socketClient, ILogManager? logManager = null)
        {
            this.logManager = logManager;
            this.client = socketClient;

            this.client.ConnectCompleted += this.Client_ConnectCompleted;
            this.client.ConnectFailed += this.Client_ConnectFailed;
            this.client.Disconnected += this.Client_Disconnected;
            this.client.ReceiveCompleted += this.Client_ReceiveCompleted;
            this.client.ReceiveFailed += this.Client_ReceiveFailed;
            this.client.SendCompleted += this.Client_SendCompleted;
            this.client.SendFailed += this.Client_SendFailed;

            this.dataBuffer = new StringBuilder();
            this.log = this.logManager?.GetLogger(typeof(IrcClient));
            this.chanModeDict = this.CompileChannelModeDictionary();
            this.userModeDict = this.CompileUserModeDictionary();

            this.CurrentChannels = new List<string>(0);
            this.CurrentNickname = string.Empty;
            this.NickName = string.Empty;
            this.NickNameAlt = string.Empty;
            this.RealName = string.Empty;
            this.UserModes = new IrcUserMode[0];
            this.Username = string.Empty;
        }

        /*/ Events /*/

        /// <inheritdoc/>
        public event EventHandler<ChannelUserEventArgs>? ChannelJoined;

        /// <inheritdoc/>
        public event EventHandler<ChannelUserEventArgs>? ChannelMessage;

        /// <inheritdoc/>
        public event EventHandler<ChannelModeChangedEventArgs>? ChannelModeChanged;

        /// <inheritdoc/>
        public event EventHandler<ChannelUserEventArgs>? ChannelParted;

        /// <inheritdoc/>
        public event EventHandler? Connected;

        /// <inheritdoc/>
        public event EventHandler? Disconnected;

        /// <inheritdoc/>
        public event EventHandler<HostHiddenEventArgs>? HostHidden;

        /// <inheritdoc/>
        public event EventHandler<MessageReceivedArgs>? MessageReceived;

        /// <inheritdoc/>
        public event EventHandler<UserEventArgs>? PrivateMessage;

        /// <inheritdoc/>
        public event EventHandler<IrcReply>? ReplyReceived;

        /// <inheritdoc/>
        public event EventHandler<UserModeChangedEventArgs>? UserModeChanged;

        /// <inheritdoc/>
        public event EventHandler<UserEventArgs>? UserQuit;

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
        public IrcNetwork Network { get; set; }

        /// <inheritdoc/>
        public string RealName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IrcUserMode> UserModes { get; protected set; }

        /// <inheritdoc/>
        public string Username { get; set; }

        /*/ Methods /*/

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
        public void Connect(string serverHost, int serverPort = DefaultPort, string? serverPassword = null)
        {
            Dbg.Assert(!string.IsNullOrEmpty(serverHost), "serverHost == null||empty");
            Dbg.Assert(serverPort > 1024, "serverPort <= 1024");

            this.serverHost = serverHost;
            this.serverPassword = serverPassword;
            this.serverPort = serverPort;

            this.Log(Info, $"Attempting to connect to {serverHost} on port {serverPort}");

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
            Dbg.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");
            this.Send($"JOIN {channel}{CrLf}");
        }

        /// <inheritdoc/>
#pragma warning disable SA1011 // Closing square brackets must be spaced correctly
        public void Join(string[] channels, string[]? channelKeys = null)
        {
            Dbg.Assert(channels != null && channels.Length > 0, "channels == null || empty");

            var chans = string.Join(",", channels);
            var keys = (channelKeys != null) ? " " + string.Join(",", channelKeys) : string.Empty;

            this.Send($"JOIN {chans}{keys}{CrLf}");
        }
#pragma warning restore SA1011 // Closing square brackets must be spaced correctly

        /// <summary>
        /// Looks up a channel mode.
        /// </summary>
        /// <param name="mode">Resolved mode.</param>
        /// <returns>A tuple with the channel mode, has add parameter, and has remove parameter.</returns>
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
        public (IrcChannelMode Mode, bool HasAddParam, bool HasRemParam) LookupChannelMode(char mode)
        {
            return (
                this.chanModeDict[mode].Item1,
                this.chanModeDict[mode].Item2,
                this.chanModeDict[mode].Item3);
        }
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly

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
            Dbg.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");

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
            this.Send(Encoding.UTF8.GetBytes(data));
        }

        /// <inheritdoc/>
        public void Send(string format, params object[] args)
        {
            this.Send(Encoding.UTF8.GetBytes(string.Format(format, args)));
        }

        /// <inheritdoc/>
        public void Send(byte[] data)
        {
            this.Log(Debug, $"<< Sending {data.Length} bytes");
            this.client.Send(data);
        }

        /// <inheritdoc/>
        public void SendMessage(string to, string format, params object[] args)
        {
            var msg = string.Format(format, args);

            foreach (var segment in this.GetMessageSegments(msg))
            {
                this.Send($"PRIVMSG {to} :{segment}{CrLf}");
            }
        }

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
            this.CurrentNickname = this.NickName;
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

        private void Client_ConnectCompleted(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.serverPassword))
            {
                this.Send($"PASS {this.serverPassword}{CrLf}");
            }

            this.Send($"NICK {this.NickName}{CrLf}USER {this.Username} 0 * :{this.Username}{CrLf}");
        }

        private void Client_ConnectFailed(object? sender, EventArgs e)
        {
            this.Log(Error, "Connection failed");
        }

        private void Client_Disconnected(object? sender, EventArgs e)
        {
            this.Log(Warn, $"Client disconnected");
        }

        private void Client_ReceiveCompleted(object? sender, ReceiveCompletedEventArgs e)
        {
            if (this.client == null)
            {
                return;
            }

            this.OnDataReceived(new DataReceivedArgs(e.Data));
            this.HandleData(e.Data);
        }

        private void Client_ReceiveFailed(object? sender, SocketEventArgs e)
        {
            this.Log(Error, $"Receive failed ({e.Error})");
        }

        private void Client_SendCompleted(object? sender, EventArgs e)
        {
            this.Log(Debug, $"Send completed");
        }

        private void Client_SendFailed(object? sender, SocketEventArgs e)
        {
            this.Log(Error, $"Send failed ({e.Error})");
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

        private string[] GetMessageSegments(string msg)
        {
            var segments = msg.Length / MessageSizeLimit;
            if (segments == 0)
            {
                return new string[] { msg };
            }

            var result = new string[segments + 1];
            for (var x = 0; x < result.Length; ++x)
            {
                result[x] = msg.Substring(x * 484, (x < segments) ? 484 : msg.Length - (x * 484));
            }

            return result;
        }

        private void HandleData(byte[] data)
        {
            var incoming = Encoding.UTF8.GetString(data);
            this.dataBuffer.Append(incoming);

            while (this.dataBuffer.ToString().Contains(CrLf))
            {
                var temp = this.dataBuffer.ToString();
                var rnIndex = temp.IndexOf(CrLf);
                var length = temp.Length - (temp.Length - rnIndex);
                var message = temp.Substring(0, length);

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

            var msgParts = message.Split(' ');
            switch (msgParts[0].Trim().ToUpperInvariant())
            {
                case "NOTICE":
                    break;
                case "PING":
                    this.Log(Info, ">> PING");
                    var pingSource = msgParts[1].Replace(":", string.Empty);
                    this.Send($"PONG {pingSource}{CrLf}");
                    this.Log(Info, "<< PONG");
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
                    // :<server> 396 <nickname> <new.hostname.tld> :is now your hidden host
                    this.OnHostHidden(new HostHiddenEventArgs(reply.Params?[0] ?? string.Empty));
                    break;
                }

                case "JOIN":
                {
                    // :<nick>!<user>@<host> JOIN <channel>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = false;

                    if (user.Nickname == this.CurrentNickname)
                    {
                        this.CurrentChannels.Add(reply.Target);
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
                        var ops = reply.Params?[0];
                        var ctr = '\0';

                        for (int x = 0, c = 0; x < ops?.Length; ++x)
                        {
                            if ("-+".Contains(ops[x]))
                            {
                                ctr = ops[x];
                                continue;
                            }

                            var mode = this.LookupChannelMode(ops[x]);

                            if (ctr == '+')
                            {
                                var val = mode.HasAddParam ? reply.Params?[++c] : null;
                                add.Add(new IrcChannelModeValue(mode.Mode, val));
                            }
                            else if (ctr == '-')
                            {
                                var val = mode.HasRemParam ? reply.Params?[++c] : null;
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

                    if (user.Nickname == this.CurrentNickname)
                    {
                        this.CurrentChannels.Remove(reply.Target);
                        isOwned = true;
                    }

                    this.OnChannelParted(new ChannelUserEventArgs(reply.Target, user, isOwned, reply.Trailing, IrcMessageType.None));
                    break;
                }

                case "PRIVMSG":
                case "NOTICE":
                {
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = user.Nickname == this.CurrentNickname;
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
                    var isOwned = user.Nickname == this.CurrentNickname;

                    this.OnUserQuit(new UserEventArgs(user, reply.Trailing, isOwned));
                    break;
                }
            }
        }

        private void Log(int level, string? text = null, Exception? exc = null)
        {
            if (this.log == null) { return; }

            var qualifiedText = $"[{this.serverHost}] {text}";

            switch (level)
            {
                case Debug: this.log.Debug(qualifiedText, exc); break;
                case Info: this.log.Info(qualifiedText, exc); break;
                case Warn: this.log.Warn(qualifiedText, exc); break;
                case Error: this.log.Error(qualifiedText, exc); break;
                case Fatal: this.log.Fatal(qualifiedText, exc); break;
                default: throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}
