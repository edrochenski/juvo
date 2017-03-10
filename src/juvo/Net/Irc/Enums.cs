using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public enum ClientState
    {
        None,
        Connecting,
        Connected
    }

    public enum MessageType
    {
        None,
        PrivateMessage,
        Notice
    }
}
