using System;
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
        public const int    BufferSize      = 4096;
        public const string ChannelIdents   = "&#+!";
        public const string CrLf            = "\r\n";
        public const int    DefaultPort     = 6667;

    /*/ Fields /*/
        readonly ILogger        logger;
        readonly ILoggerFactory loggerFactory;

        SocketClient    client;
        Thread          clientReceiveThread;
        Thread          clientSendThread;
        ClientState     clientState;
        List<string>    currentChannels;
        string          currentNickname;
        StringBuilder   dataBuffer;
        string          nickname;
        string          nicknameAlt;
        string          realName;
        string          serverHost;
        int             serverPort;
        IPHostEntry     serverEntry;
        string          username;

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
        public string RealName
        {
            get { return realName; }
            set { realName = value; }
        }
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
        public event EventHandler<MessageReceivedArgs> MessageReceived;
        public event EventHandler<UserEventArgs> PrivateMessage;
        public event EventHandler<IrcReply> ReplyReceived;
        public event EventHandler<UserEventArgs> UserQuit;

    /*/ Con/Destructors /*/
        public IrcClient(ILoggerFactory loggerFactory = null)
        {
            this.client = new SocketClient();
            this.client.ConnectCompleted += Client_ConnectCompleted;
            this.client.ConnectFailed += Client_ConnectFailed;
            this.client.Disconnected += Client_Disconnected;
            this.client.ReceiveCompleted += Client_ReceiveCompleted;
            this.client.ReceiveFailed += Client_ReceiveFailed;
            this.client.SendCompleted += Client_SendCompleted;
            this.client.SendFailed += Client_SendFailed;

            this.dataBuffer = new StringBuilder();
            this.clientState = ClientState.None;
            this.currentChannels = new List<string>(0);
            this.loggerFactory = loggerFactory;

            if (this.loggerFactory != null)
            { this.logger = this.loggerFactory.CreateLogger<IrcClient>(); }
        }

    /*/ Public Methods /*/
        public void Connect(string serverHost, int serverPort = DefaultPort)
        {
            Debug.Assert(!String.IsNullOrEmpty(serverHost), "serverHost == null||empty");
            Debug.Assert(serverPort > 1024, "serverPort <= 1024");

            this.serverHost = serverHost;
            this.serverPort = serverPort;
            //serverEntry = await Dns.GetHostEntryAsync(serverHost);

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
            var keys  = (channelKeys != null) ? " " + string.Join(",", channelKeys) : "";

            Send($"JOIN {chans}{keys}{CrLf}");
        }
        public void Part(string channel, string message = "")
        {
            Debug.Assert(!string.IsNullOrEmpty(channel), "channel == null||empty");

            Send("PART {0}{1}\r\n", channel,
                    (String.IsNullOrEmpty(message)) ? "" : String.Concat(" ", message));
        }
        public void Quit(string message = "")
        {
            Send("QUIT :{0}\r\n", message);
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
        { Send("PRIVMSG {0} :{1}\r\n", to, String.Format(format, args)); }

    /*/ Protected Methods /*/
        protected virtual void OnChannelJoined(ChannelUserEventArgs e)
        {
            //logger.Info("{0} joined {1}", e.User.Nickname, e.Channel);

            ChannelJoined?.Invoke(this, e);
        }
        protected virtual void OnChannelMessage(ChannelUserEventArgs e)
        {
            //logger.Debug("({0}:{1}) {2}", e.Channel, e.User.Nickname, e.Message);
            ChannelMessage?.Invoke(this, e);
        }
        protected virtual void OnChannelParted(ChannelUserEventArgs e)
        {
            //logger.Info("{0} parted {1} ({2})", e.User.Nickname, e.Channel, e.Message);
            ChannelParted?.Invoke(this, e);
        }
        protected virtual void OnConnected(EventArgs e)
        {
            //logger.Info("Connected to {0}:{1}", serverHost, serverPort);

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
        protected virtual void OnMessageReceived(MessageReceivedArgs e)
        {
            MessageReceived?.Invoke(this, e);

            HandleMessage(e.Message);
        }
        protected virtual void OnPrivateMessage(UserEventArgs e)
        {
            //logger.Info("({0}) {1}", e.User.Nickname, e.Message);
            PrivateMessage?.Invoke(this, e);
        }
        protected virtual void OnReplyReceived(IrcReply e)
        {
            ReplyReceived?.Invoke(this, e);
        }
        protected virtual void OnUserQuit(UserEventArgs e)
        {
            //logger.Info("{0} quit ({1})", e.User.Nickname, e.Message);
            UserQuit?.Invoke(this, e);
        }

    /*/ Private Methods /*/
        private void Client_ConnectCompleted(object sender, EventArgs e)
        {
            logger?.LogInformation("Connected");
            Send($"NICK {nickname}\r\nUSER {username} 0 * :{username}\r\n");

            //Send(String.Concat("NICK ", nickname, "\r\n"));
            //Send(String.Concat("USER ", username, " 0 * :", username, "\r\n"));
        }
        private void Client_ConnectFailed(object sender, EventArgs e)
        {
            logger?.LogInformation("Connection failed");
        }
        private void Client_Disconnected(object sender, EventArgs e)
        {
            logger?.LogInformation($"Disconnected");
        }
        private void Client_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            logger?.LogDebug("Receive completed");

            if (client == null) { return; }

            OnDataReceived(new DataReceivedArgs(e.Data));
            HandleData(e.Data);
        }
        private void Client_ReceiveFailed(object sender, SocketEventArgs e)
        {
            logger?.LogError($"Receive failed ({e.Error})");
        }
        private void Client_SendCompleted(object sender, EventArgs e)
        {
            logger?.LogDebug("Send completed");
        }
        private void Client_SendFailed(object sender, SocketEventArgs e)
        {
            logger?.LogError($"Send failed ({e.Error})");
        }
        void HandleData(byte[] data)
        {
            string incoming = UTF8Encoding.UTF8.GetString(data);
            logger.LogTrace($"HandleData(): incoming\r\n{incoming}");

            dataBuffer.Append(incoming);
            logger.LogTrace($"HandleData(): dataBuffer\r\n{dataBuffer}");

            while (dataBuffer.ToString().Contains("\r\n"))
            {
                string temp = dataBuffer.ToString();
                int rnIndex = temp.IndexOf("\r\n");
                int length = temp.Length - (temp.Length - rnIndex);

                string message = temp.Substring(0, length);
                logger.LogTrace($"HandleData(): message\r\n{message}");
                OnMessageReceived(new MessageReceivedArgs(message));

                dataBuffer.Remove(0, length + 2);
            }
        }
        void HandleMessage(string message)
        {
            logger?.LogDebug($"HandleMessage(): {message}");
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
            logger?.LogDebug($"HandleReply(): {message}");
            if (String.IsNullOrEmpty(message)) { return; }

            var reply = new IrcReply(message);
            var cmd = reply.Command.ToUpperInvariant();
            OnReplyReceived(reply);

            switch (cmd)
            {
                case "001":
                    {
                        OnConnected(EventArgs.Empty);
                    }
                    break;
                case "JOIN": //:<nick>!<user>@<host> JOIN <channel>
                    {
                        var user = new IrcUser(reply.Prefix);
                        var isOwned = false;

                        if (user.Nickname == currentNickname)
                        {
                            currentChannels.Add(reply.Target);
                            isOwned = true;
                        }

                        OnChannelJoined(new ChannelUserEventArgs(reply.Target, user, isOwned));
                    }
                    break;
                case "PART": //:<nick>!<user>@<host> PART <channel> :<message>
                    {
                        var user = new IrcUser(reply.Prefix);
                        var isOwned = false;

                        if (user.Nickname == currentNickname)
                        {
                            currentChannels.Remove(reply.Target);
                            isOwned = true;
                        }

                        OnChannelParted(new ChannelUserEventArgs(reply.Target, user, isOwned, reply.Trailing, MessageType.None));
                    }
                    break;
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
                    }
                    break;
                case "QUIT": //:<nick>!<user>@<host> QUIT :<message>
                    {
                        var user = new IrcUser(reply.Prefix);
                        var isOwned = user.Nickname == currentNickname;

                        OnUserQuit(new UserEventArgs(user, reply.Trailing, isOwned));
                    }
                    break;
            }
        }
    }
}
