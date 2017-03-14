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

    public enum IrcNetwork
    {
        Undernet,
        Unknown
    }

    public enum MessageType
    {
        None,
        PrivateMessage,
        Notice
    }

    public enum UserMode
    {
        Away,           // a
        Deaf,           // d
        HiddenHost,     // x(undernet)
        Invisible,      // i
        Operator,       // o
        OperatorLocal,  // 0
        Restricted,     // r(!undernet)
        ServerNotices,  // s
        Wallops,        // w
    }
}
