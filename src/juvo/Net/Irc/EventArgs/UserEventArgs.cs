using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public class UserEventArgs : EventArgs
    {
        bool isOwned;
        string message;
        IrcMessageType messageType;
        IrcUser user;

        public bool IsOwned { get { return isOwned; } }
        public string Message { get { return message; } }
        public IrcMessageType MessageType { get { return messageType; } }
        public IrcUser User { get { return user; } }

        public UserEventArgs(IrcUser user, string message, bool isOwned)
            : this(user, message, isOwned, IrcMessageType.None) { }
        public UserEventArgs(IrcUser user, string message, bool isOwned, IrcMessageType messageType)
        {
            this.isOwned = isOwned;
            this.message = message;
            this.messageType = messageType;
            this.user = user;
        }
    }
}
