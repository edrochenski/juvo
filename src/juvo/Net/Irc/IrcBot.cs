using AngleSharp;
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
        IrcConfigConnection configFile;
        IrcClient           ircClient;
        ILogger             logger;

    /*/ Constructors /*/
        public IrcBot(IrcConfigConnection configFile, ILoggerFactory loggerFactory = null)
        {
            this.ircClient     = new IrcClient(loggerFactory);
            this.configFile    = configFile;
            this.loggerFactory = loggerFactory;

            if (loggerFactory != null)
            { logger = loggerFactory.CreateLogger<IrcBot>(); }

            ircClient.NickName = "juvo";
            ircClient.NickNameAlt = "juvo-";
            ircClient.RealName = "juvo";
            ircClient.Username = "juvo";

            this.commandToken = "!";

            ircClient.ChannelJoined += IrcClient_ChannelJoined;
            ircClient.ChannelMessage += IrcClient_ChannelMessage;
            ircClient.ChannelParted += IrcClient_ChannelParted;
            ircClient.Connected += IrcClient_Connected;
            ircClient.DataReceived += IrcClient_DataReceived;
            ircClient.Disconnected += IrcClient_Disconnected;
            ircClient.MessageReceived += IrcClient_MessageReceived;
            ircClient.PrivateMessage += IrcClient_PrivateMessage;
            ircClient.UserQuit += IrcClient_UserQuit;
        }

    /*/ Public Methods /*/
        public void Connect()
        {
            ircClient.Connect(configFile.Servers.First().Host, configFile.Servers.First().Port);
        }

    /*/ Protected Methods /*/
        protected virtual void CommandJoin(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2) { return; }
            if (ircClient.CurrentChannels.Contains(cmdArgs[1])) { return; }

            if (!cmdArgs[1].Contains(","))
            {
                if (cmdArgs.Length > 2)
                { ircClient.Join(cmdArgs[1], cmdArgs[2]); }
                else
                { ircClient.Join(cmdArgs[1]); }
            }
            else
            {
                string[] chans = cmdArgs[1].Split(',');
                string[] keys  = (cmdArgs.Length > 2) ? cmdArgs[2].Split(',') : null;

                if (keys == null || chans.Length == keys.Length)
                { ircClient.Join(chans, keys); }
            }
        }
        protected virtual void CommandQuit(string[] cmdArgs)
        {
            var msg = (cmdArgs.Length > 1) ? string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1) : "";
            ircClient.Quit(msg);
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
        void IrcClient_ChannelJoined(object sender, ChannelUserEventArgs e)
        {
            logger.LogInformation($"[{configFile.Name}] {e.User.Nickname} joined {e.Channel}");
        }
        void IrcClient_ChannelMessage(object sender, ChannelUserEventArgs e)
        {
            logger.LogDebug($"[{configFile.Name}] <{e.Channel}\\{e.User.Nickname}> {e.Message}");
            
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
        void IrcClient_ChannelParted(object sender, ChannelUserEventArgs e)
        {
            logger.LogInformation($"[{configFile.Name}] {e.User.Nickname} parted {e.Channel}");
        }
        void IrcClient_Connected(object sender, EventArgs e)
        {
            logger.LogInformation($"[{configFile.Name}] Connected to server");
            JoinAllChannels();
        }
        void IrcClient_DataReceived(object sender, EventArgs e)
        {
        }
        void IrcClient_Disconnected(object sender, EventArgs e)
        {
            logger.LogInformation($"[{configFile.Name}] Disconnected to server");
        }
        void IrcClient_MessageReceived(object sender, MessageReceivedArgs e)
        {
            logger.LogTrace($"[{configFile.Name}] MSG: {e.Message}");
        }
        void IrcClient_PrivateMessage(object sender, UserEventArgs e)
        {
            logger.LogDebug($"[{configFile.Name}] <PRIVMSG\\{e.User.Nickname}> {e.Message}");
        }
        void IrcClient_UserQuit(object sender, UserEventArgs e)
        {
            logger.LogDebug($"[{configFile.Name}] {e.User.Nickname} quit");
        }
        void JoinAllChannels()
        {
            if (configFile != null && configFile.Channels != null)
            {
                foreach (var chan in configFile.Channels)
                { ircClient.Join(chan.Name); }
            }
        }
    }
}
