using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using juvo.Irc;
using juvo.Logging;

namespace juvo
{
    public class IrcBot : IrcClient
    {
    /*/ Constants /*/
        public const string DefaultCommandToken = "!";

    /*/ Fields /*/
        string   commandToken;
        FileInfo configFile;

    /*/ Constructors /*/
        public IrcBot(string configFilePath, ILogger logger = null) : base(logger)
        {
            base.NickName = "juvo";
            base.NickNameAlt = "juvo-";
            base.RealName = "juvo";
            base.Username = "juvo";
            this.commandToken = "!";
        }

    /*/ Public Methods /*/


    /*/ Protected Methods /*/
        protected override void OnConnected(EventArgs e)
        {
            logger.Info("Connected to server");
            base.OnConnected(e);
        }
        protected override void OnMessageReceived(MessageReceivedArgs e)
        {
            logger.Trace($"MSG: {e.Message}");
            base.OnMessageReceived(e);
        }
        protected override void OnChannelJoined(ChannelUserEventArgs e)
        {
            logger.Trace($"{e.User.Nickname} joined {e.Channel}");
            base.OnChannelJoined(e);
        }
        protected override void OnChannelMessage(ChannelUserEventArgs e)
        {
            logger.Trace($"<{e.Channel}\\{e.User.Nickname}> {e.Message}");
            base.OnChannelMessage(e);
        }
        protected override void OnChannelParted(ChannelUserEventArgs e)
        {
            logger.Trace($"{e.User.Nickname} parted {e.Channel}");
            base.OnChannelParted(e);
        }
        protected override void OnPrivateMessage(UserEventArgs e)
        {
            logger.Trace($"<PRIVMSG\\{e.User.Nickname}> {e.Message}");
            base.OnPrivateMessage(e);
        }
        protected override void OnUserQuit(UserEventArgs e)
        {
            logger.Trace($"{e.User.Nickname} quit");
            base.OnUserQuit(e);
        }

        /*/ Private Methods /*/

    }
}
