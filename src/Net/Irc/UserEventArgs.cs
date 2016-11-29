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
        MessageType messageType;
        IrcUser user;

        public bool IsOwned { get { return isOwned; } }
        public string Message { get { return message; } }
        public MessageType MessageType { get { return messageType; } }
        public IrcUser User { get { return user; } }

        public UserEventArgs(IrcUser user, string message, bool isOwned)
            : this(user, message, isOwned, MessageType.None) { }
        public UserEventArgs(IrcUser user, string message, bool isOwned, MessageType messageType)
        {
            this.isOwned = isOwned;
            this.message = message;
            this.messageType = messageType;
            this.user = user;
        }
    }
}
