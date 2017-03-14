﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

namespace Juvo.Net.Irc
{
    public class IrcClient : IDisposable
    {
    /*/ Constants /*/
        public const int BufferSize = 4096;
        public const string ChannelIdents = "&#+!";
        public const string CrLf = "\r\n";
        public const int DefaultPort = 6667;

    /*/ Fields /*/
        readonly ILogger logger;
        readonly ILoggerFactory loggerFactory;
        readonly Dictionary<char, UserMode> userModeDict;

        SocketClient client;
        Thread clientReceiveThread;
        Thread clientSendThread;
        ClientState clientState;
        List<string> currentChannels;
        string currentNickname;
        StringBuilder dataBuffer;
        string nickname;
        string nicknameAlt;
        string realName;
        string serverHost;
        int serverPort;
        IPHostEntry serverEntry;
        string username;

    /*/ Properties /*/
        public List<string> CurrentChannels
        {
            get { return currentChannels; }
        }
        public string CurrentNickname
        {
            get { return currentNickname; }
        }
        public string NickName
        {
            get { return nickname; }
            set { nickname = value; }
        }
        public string NickNameAlt
        {
            get { return nicknameAlt; }
            set { nicknameAlt = value; }
        }
        public IrcNetwork Network { get; protected set; }
        public string RealName
        {
            get { return realName; }
            set { realName = value; }
        }
        public IEnumerable<UserMode> UserModes { get; protected set; }
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

    /*/ Events /*/
        public event EventHandler<ChannelUserEventArgs> ChannelJoined;
        public event EventHandler<ChannelUserEventArgs> ChannelMessage;
        public event EventHandler<ChannelUserEventArgs> ChannelParted;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler DataReceived;
        public event EventHandler<HostHiddenEventArgs> HostHidden;
        public event EventHandler<MessageReceivedArgs> MessageReceived;
        public event EventHandler<UserEventArgs> PrivateMessage;
        public event EventHandler<IrcReply> ReplyReceived;
        public event EventHandler<UserModeChangedEventArgs> UserModeChanged;
        public event EventHandler<UserEventArgs> UserQuit;

    /*/ Con/Destructors /*/
        public IrcClient(ILoggerFactory loggerFactory = null) : this(IrcNetwork.Unknown, loggerFactory) { }
        public IrcClient(IrcNetwork network, ILoggerFactory loggerFactory = null)
        {
            client = new SocketClient();
            client.ConnectCompleted += Client_ConnectCompleted;
            client.ConnectFailed += Client_ConnectFailed;
            client.Disconnected += Client_Disconnected;
            client.ReceiveCompleted += Client_ReceiveCompleted;
            client.ReceiveFailed += Client_ReceiveFailed;
            client.SendCompleted += Client_SendCompleted;
            client.SendFailed += Client_SendFailed;

            dataBuffer = new StringBuilder();
            clientState = ClientState.None;
            currentChannels = new List<string>(0);

            Network = network;

            this.loggerFactory = loggerFactory;
            if (this.loggerFactory != null)
            { logger = this.loggerFactory.CreateLogger<IrcClient>(); }

            userModeDict = CompileUserModeDictionary();
        }

    /*/ Public Methods /*/
        public void Connect(string serverHost, int serverPort = DefaultPort)
        {
            Debug.Assert(!String.IsNullOrEmpty(serverHost), "serverHost == null||empty");
            Debug.Assert(serverPort > 1024, "serverPort <= 1024");

            this.serverHost = serverHost;
            this.serverPort = serverPort;

            logger?.LogInformation($"Attempting to connect to {serverHost} on port {serverPort}");

            clientState = ClientState.Connecting;
            client.Connect(this.serverHost, this.serverPort);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }
        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null) client.Dispose();
            }
        }
        public void Join(string channel, string key = "")
        {
            Debug.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");
            Send($"JOIN {channel}{CrLf}");
        }
        public void Join(string[] channels, string[] channelKeys = null)
        {
            Debug.Assert(channels != null && channels.Length > 0);
            Debug.Assert(channelKeys == null || channelKeys.Length == channels.Length);

            var chans = string.Join(",", channels);
            var keys = (channelKeys != null) ? " " + string.Join(",", channelKeys) : "";

            Send($"JOIN {chans}{keys}{CrLf}");
        }
        public UserMode LookupUserMode(char mode)
        {
            return userModeDict[mode];
        }
        public IEnumerable<UserMode> LookupUserModes(char[] mode)
        {
            var result = new UserMode[mode.Length];
            for (var x = 0; x < mode.Length; ++x)
            { result[x] = userModeDict[mode[x]]; }

            return result;
        }
        public IEnumerable<UserMode> LookupUserModes(string mode)
        {
            return LookupUserModes(mode.ToCharArray());
        }
        public void Part(string channel, string message = "")
        {
            Debug.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");

            var msg = (String.IsNullOrEmpty(message)) ? "" : String.Concat(" ", message);
            Send($"PART {channel}{msg}{CrLf}");
        }
        public void Quit(string message = "")
        {
            Send($"QUIT :{message}{CrLf}");
        }
        public void Send(string data)
        { client.Send(UTF8Encoding.UTF8.GetBytes(data)); }
        public void Send(string format, params object[] args)
        { client.Send(string.Format(format, args)); }
        public void Send(byte[] data)
        {
            client.Send(data);
        }
        public void SendMessage(string to, string format, params object[] args)
        {
            var msg = String.Format(format, args);
            Send($"PRIVMSG {to} :{msg}{CrLf}");
        }

    /*/ Protected Methods /*/
        protected virtual void OnChannelJoined(ChannelUserEventArgs e)
        {
            ChannelJoined?.Invoke(this, e);
        }
        protected virtual void OnChannelMessage(ChannelUserEventArgs e)
        {
            ChannelMessage?.Invoke(this, e);
        }
        protected virtual void OnChannelParted(ChannelUserEventArgs e)
        {
            ChannelParted?.Invoke(this, e);
        }
        protected virtual void OnConnected(EventArgs e)
        {
            clientState = ClientState.Connected;
            currentNickname = nickname; //TODO: don't assume the first nick worked

            Connected?.Invoke(this, e);
        }
        protected virtual void OnDataReceived(DataReceivedArgs e)
        {

        }
        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }
        protected virtual void OnHostHidden(HostHiddenEventArgs e)
        {
            HostHidden?.Invoke(this, e);
        }
        protected virtual void OnMessageReceived(MessageReceivedArgs e)
        {
            MessageReceived?.Invoke(this, e);
            HandleMessage(e.Message);
        }
        protected virtual void OnPrivateMessage(UserEventArgs e)
        {
            PrivateMessage?.Invoke(this, e);
        }
        protected virtual void OnReplyReceived(IrcReply e)
        {
            ReplyReceived?.Invoke(this, e);
        }
        protected virtual void OnUserModeChanged(UserModeChangedEventArgs e)
        {
            UserModeChanged?.Invoke(this, e);
        }
        protected virtual void OnUserQuit(UserEventArgs e)
        {
            UserQuit?.Invoke(this, e);
        }

    /*/ Private Methods /*/
        void Client_ConnectCompleted(object sender, EventArgs e)
        {
            Send($"NICK {nickname}\r\nUSER {username} 0 * :{username}\r\n");
        }
        void Client_ConnectFailed(object sender, EventArgs e)
        {
            logger?.LogError("Connection failed");
        }
        void Client_Disconnected(object sender, EventArgs e)
        {

        }
        void Client_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            if (client == null) { return; }

            OnDataReceived(new DataReceivedArgs(e.Data));
            HandleData(e.Data);
        }
        void Client_ReceiveFailed(object sender, SocketEventArgs e)
        {
            logger?.LogError($"Receive failed ({e.Error})");
        }
        void Client_SendCompleted(object sender, EventArgs e)
        {

        }
        void Client_SendFailed(object sender, SocketEventArgs e)
        {
            logger?.LogError($"Send failed ({e.Error})");
        }
        Dictionary<char, UserMode> CompileUserModeDictionary()
        {
            var result = new Dictionary<char, UserMode>()
            {
                { 'a', UserMode.Away },
                { 'd', UserMode.Deaf },
                { 'i', UserMode.Invisible },
                { 'o', UserMode.Operator },
                { 'r', UserMode.Restricted },
                { 's', UserMode.ServerNotices },
                { 'w', UserMode.Wallops },
                { '0', UserMode.OperatorLocal }
            };

            if (Network == IrcNetwork.Undernet)
            {
                result.Add('x', UserMode.HiddenHost);
            }

            return result;
        }
        void HandleData(byte[] data)
        {
            string incoming = UTF8Encoding.UTF8.GetString(data);
            dataBuffer.Append(incoming);

            while (dataBuffer.ToString().Contains("\r\n"))
            {
                string temp = dataBuffer.ToString();
                int rnIndex = temp.IndexOf("\r\n");
                int length = temp.Length - (temp.Length - rnIndex);

                string message = temp.Substring(0, length);
                OnMessageReceived(new MessageReceivedArgs(message));

                dataBuffer.Remove(0, length + 2);
            }
        }
        void HandleMessage(string message)
        {
            if (String.IsNullOrEmpty(message)) { return; }

            if (message.StartsWith(":"))
            {
                HandleReply(message);
                return;
            }

            string[] msgParts = message.Split(' ');
            switch (msgParts[0].Trim().ToUpperInvariant())
            {
                case "NOTICE":
                    break;
                case "PING":
                    string pingSource = msgParts[1].Replace(":", "");
                    Send("PONG {0}\r\n", pingSource);
                    break;
            }
        }
        void HandleReply(string message)
        {
            if (String.IsNullOrEmpty(message)) { return; }

            var reply = new IrcReply(message);
            var cmd = reply.Command.ToUpperInvariant();
            OnReplyReceived(reply);

            switch (cmd)
            {
                case "001":
                {
                    OnConnected(EventArgs.Empty);
                } break;
                case "396":
                {
                    //:<server> 396 enloco enloco.users.undernet.org :is now your hidden host
                    OnHostHidden(new HostHiddenEventArgs(reply.Target));
                } break;
                case "JOIN":
                {
                    //:<nick>!<user>@<host> JOIN <channel>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = false;

                    if (user.Nickname == currentNickname)
                    {
                        currentChannels.Add(reply.Target);
                        isOwned = true;
                    }

                    OnChannelJoined(new ChannelUserEventArgs(reply.Target, user, isOwned));
                } break;
                case "MODE":
                {
                    //:n!u@h MODE <target> :[+]modes[-]modes

                    var add = "";
                    var rem = "";
                    var plus = true;
                    for (var x = 0; x < reply.Trailing.Length; ++x)
                    {
                        var ch = reply.Trailing[x];
                        if      (ch == '+') { plus = true; }
                        else if (ch == '-') { plus = false; }
                        else if (plus)      { add += ch; }
                        else if (!plus)     { rem += ch; }
                    }

                    OnUserModeChanged(new UserModeChangedEventArgs(LookupUserModes(add), LookupUserModes(rem)));
                } break;
                case "PART":
                {
                    //:<nick>!<user>@<host> PART <channel> :<message>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = false;

                    if (user.Nickname == currentNickname)
                    {
                        currentChannels.Remove(reply.Target);
                        isOwned = true;
                    }

                    OnChannelParted(new ChannelUserEventArgs(reply.Target, user, isOwned, reply.Trailing, MessageType.None));
                } break;
                case "PRIVMSG":
                case "NOTICE":
                {
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = user.Nickname == currentNickname;
                    var msgType = (cmd == "PRIVMSG") ? MessageType.PrivateMessage : MessageType.Notice;

                    if (reply.TargetIsChannel)
                    {
                        OnChannelMessage(new ChannelUserEventArgs(
                            reply.Target, user, isOwned, reply.Trailing, msgType));
                    }
                    else
                    {
                        OnPrivateMessage(new UserEventArgs(
                            user, reply.Trailing, isOwned, msgType));
                    }
                } break;
                case "QUIT":
                {
                    //:<nick>!<user>@<host> QUIT :<message>
                    var user = new IrcUser(reply.Prefix);
                    var isOwned = user.Nickname == currentNickname;

                    OnUserQuit(new UserEventArgs(user, reply.Trailing, isOwned));
                } break;
            }
        }

    /*/ Static Properties /*/
        static public readonly Dictionary<string, IrcNetwork> IrcNetworkLookup;

    /*/ Static Methods /*/
        static IrcClient()
        {
            IrcNetworkLookup = new Dictionary<string, IrcNetwork>
            {
                { "undernet", IrcNetwork.Undernet }
            };
        }
        static public IrcNetwork LookupNetwork(string network)
        {
            network = (string.IsNullOrEmpty(network)) ? "" : network.ToLowerInvariant();
            return (IrcNetworkLookup.ContainsKey(network))
                    ? IrcNetworkLookup[network]
                    : IrcNetwork.Unknown;
        }
    }
}
