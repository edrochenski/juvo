using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using juvo.Logging;

namespace juvo.Irc
{
    public class IrcClient : IIrcClient, IDisposable
    {
    /*/ Constants /*/
        public const int    BufferSize      = 4096;
        public const string ChannelIdents   = "&#+!";
        public const int    DefaultPort     = 6667;

    /*/ Fields /*/
        protected readonly ILogger logger;
        TcpClient       client;
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
            protected set { nickname = value; }
        }
        public string NickNameAlt
        {
            get { return nicknameAlt; }
            protected set { nicknameAlt = value; }
        }
        public string RealName
        {
            get { return realName; }
            protected set { realName = value; }
        }
        public string Username
        {
            get { return username; }
            protected set { username = value; }
        }

    /*/ Events /*/
        public event EventHandler<ChannelUserEventArgs> ChannelJoined;
        public event EventHandler<ChannelUserEventArgs> ChannelMessage;
        public event EventHandler<ChannelUserEventArgs> ChannelParted;
        public event EventHandler Connected;
        public event EventHandler DataReceived;
        public event EventHandler MessageReceived;
        public event EventHandler<UserEventArgs> PrivateMessage;
        public event EventHandler<IrcReply> ReplyReceived;
        public event EventHandler<UserEventArgs> UserQuit;

    /*/ Con/Destructors /*/
        protected IrcClient(ILogger logger = null)
        {
            this.dataBuffer = new StringBuilder();
            this.client = new TcpClient();
            this.clientState = ClientState.None;
            this.currentChannels = new List<string>(0);
            this.logger = logger;
        }

    /*/ Public Methods /*/
        public async Task ConnectAsync(string serverHost, int serverPort = DefaultPort)
        {
            if (String.IsNullOrEmpty(serverHost)) { throw new ArgumentNullException("serverHost"); }
            if (serverPort < 0) { throw new ArgumentOutOfRangeException("serverPort"); }

            this.serverHost = serverHost;
            this.serverPort = serverPort;
            serverEntry = await Dns.GetHostEntryAsync(serverHost);

            //logger.Info("Attempting to connect to {0} on port {1}", this.serverHost, this.serverPort);

            clientState = ClientState.Connecting;
            await client.ConnectAsync(serverEntry.AddressList, serverPort);
            await SendAsync(String.Concat("NICK ", nickname, "\r\n"));
            await SendAsync(String.Concat("USER ", username, " 0 * :", username, "\r\n"));
            await ReceiveDataAsync();
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
        public async Task Join(string channel)
        {
            if (String.IsNullOrEmpty(channel)) { throw new ArgumentNullException("channel"); }

            await SendAsync("JOIN {0}\r\n", channel);
        }
        public async Task PartAsync(string channel, string message = "")
        {
            if (String.IsNullOrEmpty(channel)) { throw new ArgumentNullException("channel"); }

            await SendAsync("PART {0}{1}\r\n", channel,
                            (String.IsNullOrEmpty(message)) ? "" : String.Concat(" ", message));
        }
        public async Task QuitAsync(string message = "")
        {
            await SendAsync("QUIT :{0}\r\n", message);
        }
        public async Task SendAsync(string data)
        { await SendAsync(UTF8Encoding.UTF8.GetBytes(data)); }
        public async Task SendAsync(string format, params object[] args)
        { await SendAsync(String.Format(format, args)); }
        public async Task SendAsync(byte[] data)
        {
            await client.GetStream().WriteAsync(data, 0, data.Length);
        }
        public async Task SendMessageAsync(string to, string format, params object[] args)
        { await SendAsync("PRIVMSG {0} :{1}\r\n", to, String.Format(format, args)); }

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
            DataReceived?.Invoke(this, e);

            HandleData(e.Data);
            ReceiveDataAsync();
        }
        protected virtual void OnMessageReceived(MessageReceivedArgs e)
        {
            MessageReceived?.Invoke(this, e);

            HandleMessageAsync(e.Message);
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
        async Task HandleMessageAsync(string message)
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
                    await SendAsync("PONG {0}\r\n", pingSource);
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
        void SocketConnectCallback(IAsyncResult result)
        {
            ReceiveDataAsync();
        }
        async Task ReceiveDataAsync()
        {
            if (client == null) { return; }

            int bytesRead = 0;
            byte[] data = new byte[IrcClient.BufferSize];
            while ((bytesRead = await client.GetStream().ReadAsync(data, 0, data.Length)) == 0) { }

            byte[] eventData = new byte[bytesRead];
            Array.Copy(data, 0, eventData, 0, bytesRead);
            OnDataReceived(new DataReceivedArgs(eventData));
        }
    }
}
