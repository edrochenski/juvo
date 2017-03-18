using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    /// <summary>
    /// Settable modes for an IRC channel.
    /// </summary>
    /// <remarks>
    ///     Charybdis: http://www.stack.nl/~jilles/irc/charybdis-oper-guide/cmodes.htm
    ///     ircu     : http://ircu.sourceforge.net/    
    /// 
    ///     DALnet  : http://docs.dal.net/docs/modes.html#2
    ///     Undernet: http://www.help.undernet.org/faq.php?what=channelmodes
    /// </remarks>
    public enum IrcChannelMode
    {
        /// <summary>
        /// No mode specified
        /// </summary>
        None,
        /// <summary>
        /// Meant for network-wide events, this mode will make it so that only
        /// operators/voiced users and their messages/events are seen in the channel.
        /// </summary>
        Auditorium,     // A (dalnet)
        /// <summary>
        /// Channel ban to deny a user entry based on the u!n@h mask
        /// </summary>
        Ban,            // b *
        /// <summary>
        /// Prevents messages (dalnet) or modifies messages (charybdis) with any control characters 
        /// (for color, bolding, ...) in the channel
        /// </summary>
        Colorless,      // c (dalnet)
        /// <summary>
        /// Channel will not show users who join until some action happens with them (msg, opped/voiced, ...)
        /// </summary>
        DelayJoin,      // D (undernet)
        /// <summary>
        /// Channel exception that overrides a banned mask to allow a user entry
        /// </summary>
        Exception,      // e *
        /// <summary>
        /// Forwards users to another channel when they join, sending them a 470 reply
        /// with original and target channel
        /// </summary>
        Forward,        // f (charybdis)
        /// <summary>
        /// Allows anyone with ops in a channel to forward to this channel
        /// </summary>
        ForwardAny,     // F (charybdis)
        /// <summary>
        /// Channel invitation to allow a user entry into an invite-only channel.
        /// </summary>
        Invitation,     // I (dalnet)
        /// <summary>
        /// Allows anyone to invite to the channel
        /// </summary>
        InviteAny,      // g (charybdis)
        /// <summary>
        /// Invite-only only allows explicitly invited users into the channel
        /// </summary>
        InviteOnly,     // i *
        /// <summary>
        /// Sets a join limit on the channel to prevent a large amount of users to join in
        /// a short amount of time.  4:5 would specify that up to 4 users can join in 5 seconds.
        /// </summary>
        JoinLimit,      // j (dalnet,charybdis)
        /// <summary>
        /// Channel key/password to prevent unknown users from joining.
        /// </summary>
        Key,            // k *
        /// <summary>
        /// Channel is allowed a larger number of bans (usually only set by netoper)
        /// </summary>
        LargeBanList,   // L (charybdis)
        /// <summary>
        /// Channel user limit
        /// </summary>
        Limit,          // l *
        /// <summary>
        /// Determines if the channel will be listed when using the /list command
        /// </summary>
        Listed,         // L (bahamut)
        /// <summary>
        /// Moderated channel only allows op/voiced users to send messages
        /// </summary>
        Moderated,      // m *
        /// <summary>
        /// Messages from users that are not currently in the channel will not be sent to the channel
        /// </summary>
        NoExternal,     // n *
        /// <summary>
        /// Channel will not be a valid target for forwarding, anyone forwarded will be ignored
        /// </summary>
        NoForwarded,    // Q (charybdis)
        /// <summary>
        /// Channel operator
        /// </summary>
        Operator,       // o *
        /// <summary>
        /// Oper-only channel will only allow users with umode +O to join
        /// </summary>
        OperOnly,       // O (dalnet)
        /// <summary>
        /// Channel will not allow /KNOCK and will not be shown in /whois replies
        /// </summary>
        Paranoid,       // p (charybdis)
        /// <summary>
        /// Channel will not be destroyed when the last user leaves (netoper can only set)
        /// </summary>
        Permanent,      // P (charybdis)
        /// <summary>
        /// Channel will not be shown in a /whois unless the user is present in the channel
        /// </summary>
        Private,        // p *
        /// <summary>
        /// Works like a ban without denying the user entry to the channel
        /// </summary>
        Quiet,          // q (charybdis)
        /// <summary>
        /// Effects of +m/+b/+q are relaxed and any messages will be seen by ops (+n is unaffected)
        /// </summary>
        ReducedMod,     // z (charybdis)
        /// <summary>
        /// Channel is registered with services
        /// </summary>
        Registered,     // r (dalnet,charybdis) R (undernet)
        /// <summary>
        /// Users must be registered with services to join the channel
        /// </summary>
        RegisterForJoin,// R (dalnet) r (undernet)
        /// <summary>
        /// Users must be registered with services to send a message to the channel
        /// </summary>
        RegisterForMsg, // M (dalnet)
        /// <summary>
        /// Channel will not be shown in a /whois unless the user is present in the channel; In addition
        /// the channel will not show up in the /list
        /// </summary>
        Secret,         // s *
        /// <summary>
        /// Only users with a secure connection (SSL) will be allowed to join
        /// </summary>
        SslUsersOnly,   // S (charybdis)
        /// <summary>
        /// Only operators will be allowed to set the topic of the channel
        /// </summary>
        TopicLock,      // t *
        /// <summary>
        /// Channel voice
        /// </summary>
        Voiced         // v *
    }

    public enum IrcClientState
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

    public enum IrcMessageType
    {
        None,
        PrivateMessage,
        Notice
    }

    public enum IrcUserMode
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
