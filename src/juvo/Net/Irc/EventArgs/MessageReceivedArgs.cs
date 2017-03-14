using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public class MessageReceivedArgs : EventArgs
    {
        string message;
        public string Message { get { return message; } }

        public MessageReceivedArgs(string message)
        { this.message = message; }
    }
}
