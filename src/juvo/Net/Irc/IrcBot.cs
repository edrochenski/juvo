using AngleSharp;
using BytedownSoftware.Lib;
using Microsoft.Extensions.Logging;
using JuvoConsole;
using System;
using System.Linq;

namespace Juvo.Net.Irc
{
    public class IrcBot
    {
    /*/ Constants /*/
        public const string DefaultCommandToken = ".";

    /*/ Fields /*/
        readonly ILoggerFactory loggerFactory;

        string              commandToken;
        IrcConfigConnection config;
        IrcClient           client;
        ILogger             logger;

    /*/ Properties /*/
        public bool IsAuthenticated { get; protected set; }

    /*/ Constructors /*/
        public IrcBot(IrcConfigConnection config, ILoggerFactory loggerFactory = null)
        {
            this.config    = config;
            this.loggerFactory = loggerFactory;

            if (loggerFactory != null)
            { logger = loggerFactory.CreateLogger<IrcBot>(); }

            commandToken = "!";

            client = new IrcClient(IrcClient.LookupNetwork(this.config.Network), loggerFactory);
            client.NickName = "juvo";
            client.NickNameAlt = "juvo-";
            client.RealName = "juvo";
            client.Username = "juvo";
            client.ChannelJoined += Client_ChannelJoined;
            client.ChannelMessage += Client_ChannelMessage;
            client.ChannelParted += Client_ChannelParted;
            client.Connected += Client_Connected;
            client.DataReceived += Client_DataReceived;
            client.Disconnected += Client_Disconnected;
            client.HostHidden += Client_HostHidden;
            client.MessageReceived += Client_MessageReceived;
            client.PrivateMessage += Client_PrivateMessage;
            client.UserQuit += Client_UserQuit;
        }

    /*/ Public Methods /*/
        public void Connect()
        {
            client.Connect(config.Servers.First().Host, config.Servers.First().Port);
        }

    /*/ Protected Methods /*/
        protected void Authenticate()
        {
            if (IsAuthenticated)
            {
                logger.LogWarning("Authenticate(): Bot has already authenticated");
            }
            else if (StringUtil.AreAnyMissing(config.User, config.Pass, config.Network))
            {
                logger.LogWarning("Authenticate(): config missing user, pass, or network");
                return;
            }

            switch (config.Network.ToLowerInvariant())
            {
                case "undernet":
                {
                    client.SendMessage($"x@channels.undernet.org", $"login {config.User} {config.Pass}");
                } break;

            }
        }
        protected virtual void CommandJoin(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2) { return; }
            if (client.CurrentChannels.Contains(cmdArgs[1])) { return; }

            if (!cmdArgs[1].Contains(","))
            {
                if (cmdArgs.Length > 2)
                { client.Join(cmdArgs[1], cmdArgs[2]); }
                else
                { client.Join(cmdArgs[1]); }
            }
            else
            {
                string[] chans = cmdArgs[1].Split(',');
                string[] keys  = (cmdArgs.Length > 2) ? cmdArgs[2].Split(',') : null;

                if (keys == null || chans.Length == keys.Length)
                { client.Join(chans, keys); }
            }
        }
        protected virtual void CommandQuit(string[] cmdArgs)
        {
            var msg = (cmdArgs.Length > 1) ? string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1) : "";
            client.Quit(msg);
        }
        protected virtual void CommandTwitter(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2) { return; }

            var handle = cmdArgs[1];
            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://twitter.com/{handle}";
            var document = BrowsingContext.New(config).OpenAsync(address);

        }

    /*/ Private Methods /*/
        void Client_ChannelJoined(object sender, ChannelUserEventArgs e)
        {
            logger.LogInformation($"[{config.Name}] {e.User.Nickname} joined {e.Channel}");
        }
        void Client_ChannelMessage(object sender, ChannelUserEventArgs e)
        {
            logger.LogDebug($"[{config.Name}] <{e.Channel}\\{e.User.Nickname}> {e.Message}");
            
            if (e.Channel.ToLowerInvariant().Equals("#bytedown") && e.Message.StartsWith("."))
            {
                var cmdParts = e.Message.Split(' ');
                switch (cmdParts[0].ToLowerInvariant())
                {
                    case ".join":    CommandJoin(cmdParts); break;
                    case ".quit":    CommandQuit(cmdParts); break;
                    case ".twitter": CommandTwitter(cmdParts); break;
                }
            }
        }
        void Client_ChannelParted(object sender, ChannelUserEventArgs e)
        {
            logger.LogInformation($"[{config.Name}] {e.User.Nickname} parted {e.Channel}");
        }
        void Client_Connected(object sender, EventArgs e)
        {
            logger.LogInformation($"[{config.Name}] Connected to server");
            
            if (!StringUtil.IsMissing(config.UserMode))
            {
                logger.LogInformation($"[{config.Name}] Requeting mode: +{config.UserMode}");
                client.Send($"MODE {client.NickName} +{config.UserMode}{IrcClient.CrLf}");
            }

            if (!StringUtil.AreAnyMissing(config.User, config.Pass, config.Network))
            {
                Authenticate();
            }
            else
            {
                JoinAllChannels();
            }
        }
        void Client_DataReceived(object sender, EventArgs e)
        {
        }
        void Client_Disconnected(object sender, EventArgs e)
        {
            logger.LogInformation($"[{config.Name}] Disconnected to server");
        }
        void Client_HostHidden(object sender, HostHiddenEventArgs e)
        {
            logger.LogInformation($"Real host hidden using '{e.Host}'");
            if (config.Network.ToLowerInvariant() == "undernet")
            {
                IsAuthenticated = true;
                JoinAllChannels();
            }
        }
        void Client_MessageReceived(object sender, MessageReceivedArgs e)
        {
            logger.LogTrace($"[{config.Name}] MSG: {e.Message}");
        }
        void Client_PrivateMessage(object sender, UserEventArgs e)
        {
            logger.LogDebug($"[{config.Name}] <PRIVMSG\\{e.User.Nickname}> {e.Message}");
        }
        void Client_UserQuit(object sender, UserEventArgs e)
        {
            logger.LogDebug($"[{config.Name}] {e.User.Nickname} quit");
        }
        void JoinAllChannels()
        {
            if (config != null && config.Channels != null)
            {
                foreach (var chan in config.Channels)
                { client.Join(chan.Name); }
            }
        }
    }
}
