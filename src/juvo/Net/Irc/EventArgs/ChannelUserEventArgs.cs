using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public class ChannelUserEventArgs : EventArgs
    {
        string channel;
        bool isOwned;
        string message;
        IrcMessageType messageType;
        IrcUser user;

        public string Channel { get { return channel; } }
        public bool IsOwned { get { return isOwned; } }
        public string Message { get { return message; } }
        public IrcMessageType MessageType { get { return messageType; } }
        public IrcUser User { get { return user; } }

        public ChannelUserEventArgs(string channel, IrcUser user, bool isOwned)
            : this(channel, user, isOwned, null, IrcMessageType.None)
        { }
        public ChannelUserEventArgs(string channel, IrcUser user, bool isOwned,
            string message, IrcMessageType messageType)
        {
            this.channel = channel;
            this.isOwned = isOwned;
            this.message = message;
            this.messageType = messageType;
            this.user = user;
        }
    }
}
