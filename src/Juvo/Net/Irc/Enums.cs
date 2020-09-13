// <copyright file="Enums.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    /// <summary>
    /// Settable modes for an IRC channel.
    /// </summary>
    /// <remarks>
    /// Codes for IRC servers can be found:
    /// <see href="http://www.stack.nl/~jilles/irc/charybdis-oper-guide/cmodes.htm">Charybdis</see>,
    /// <see href="http://ircu.sourceforge.net/">ircu</see>,
    /// <see href="http://docs.dal.net/docs/modes.html#2">DALnet</see>,
    /// <see href="http://www.help.undernet.org/faq.php?what=channelmodes">Undernet</see>.
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
        /// (A: dalnet)
        /// </summary>
        Auditorium,

        /// <summary>
        /// Channel ban to deny a user entry based on the u!n@h mask (b)
        /// </summary>
        Ban,

        /// <summary>
        /// Prevents messages (dalnet) or modifies messages (charybdis) with any control characters
        /// (for color, bolding, ...) in the channel (c: dalnet|charybdis)
        /// </summary>
        Colorless,

        /// <summary>
        /// Channel will not show users who join until some action happens with them
        /// (msg, opped/voiced, ...) (D: undernet)
        /// </summary>
        DelayJoin,

        /// <summary>
        /// Channel exception that overrides a banned mask to allow a user entry (e)
        /// </summary>
        Exception,

        /// <summary>
        /// Forwards users to another channel when they join, sending them a 470 reply
        /// with original and target channel (f: charybdis)
        /// </summary>
        Forward,

        /// <summary>
        /// Allows anyone with ops in a channel to forward to this channel (F: charybdis)
        /// </summary>
        ForwardAny,

        /// <summary>
        /// Channel invitation to allow a user entry into an invite-only channel. (I: dalnet)
        /// </summary>
        Invitation,

        /// <summary>
        /// Allows anyone to invite to the channel (g: charybdis)
        /// </summary>
        InviteAny,

        /// <summary>
        /// Invite-only only allows explicitly invited users into the channel (i)
        /// </summary>
        InviteOnly,

        /// <summary>
        /// Sets a join limit on the channel to prevent a large amount of users to join in
        /// a short amount of time.  4:5 would specify that up to 4 users can join in 5 seconds.
        /// (j: dalnet|charybdis)
        /// </summary>
        JoinLimit,

        /// <summary>
        /// Channel key/password to prevent unknown users from joining. (k)
        /// </summary>
        Key,

        /// <summary>
        /// Channel is allowed a larger number of bans (usually only set by netoper)
        /// (L: charybdis)
        /// </summary>
        LargeBanList,

        /// <summary>
        /// Channel user limit (l)
        /// </summary>
        Limit,

        /// <summary>
        /// Determines if the channel will be listed when using the /list command (L: bahamut)
        /// </summary>
        Listed,

        /// <summary>
        /// Moderated channel only allows op/voiced users to send messages (m)
        /// </summary>
        Moderated,

        /// <summary>
        /// Messages from users that are not currently in the channel will not be sent to the
        /// channel (n)
        /// </summary>
        NoExternal,

        /// <summary>
        /// Channel will not be a valid target for forwarding, anyone forwarded will be ignored
        /// (Q: charybdis)
        /// </summary>
        NoForwarded,

        /// <summary>
        /// Channel operator (o)
        /// </summary>
        Operator,

        /// <summary>
        /// Oper-only channel will only allow users with umode +O to join (O: dalnet)
        /// </summary>
        OperOnly,

        /// <summary>
        /// Channel will not allow /KNOCK and will not be shown in /whois replies (p: charybdis)
        /// </summary>
        Paranoid,

        /// <summary>
        /// Channel will not be destroyed when the last user leaves (netoper can only set)
        /// (P: charybdis)
        /// </summary>
        Permanent,

        /// <summary>
        /// Channel will not be shown in a /whois unless the user is present in the channel (p)
        /// </summary>
        Private,

        /// <summary>
        /// Works like a ban without denying the user entry to the channel (q: charybdis)
        /// </summary>
        Quiet,

        /// <summary>
        /// Effects of +m/+b/+q are relaxed and any messages will be seen by ops (+n is unaffected)
        /// (z: charybdis)
        /// </summary>
        ReducedMod,

        /// <summary>
        /// Channel is registered with services (r: dalnet|charybdis, R: undernet)
        /// </summary>
        Registered,

        /// <summary>
        /// Users must be registered with services to join the channel (R: dalnet, r: undernet)
        /// </summary>
        RegisterForJoin,

        /// <summary>
        /// Users must be registered with services to send a message to the channel (M: dalnet)
        /// </summary>
        RegisterForMsg,

        /// <summary>
        /// Channel will not be shown in a /whois unless the user is present in the channel; In addition
        /// the channel will not show up in the /list (s)
        /// </summary>
        Secret,

        /// <summary>
        /// Only users with a secure connection (SSL) will be allowed to join
        /// (S on charybdis)
        /// </summary>
        SslUsersOnly,

        /// <summary>
        /// Only operators will be allowed to set the topic of the channel (t)
        /// </summary>
        TopicLock,

        /// <summary>
        /// Channel voice (v)
        /// </summary>
        Voiced
    }

    /// <summary>
    /// IRC client state.
    /// </summary>
    public enum IrcClientState
    {
        /// <summary>
        /// None/Unknown.
        /// </summary>
        None,

        /// <summary>
        /// Connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// Connected.
        /// </summary>
        Connected
    }

    /// <summary>
    /// IRC network.
    /// </summary>
    public enum IrcNetwork
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Undernet.
        /// </summary>
        Undernet
    }

    /// <summary>
    /// IRC message type.
    /// </summary>
    public enum IrcMessageType
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Private message.
        /// </summary>
        PrivateMessage,

        /// <summary>
        /// Notice.
        /// </summary>
        Notice
    }

    /// <summary>
    /// IRC user mode.
    /// </summary>
    public enum IrcUserMode
    {
        /// <summary>
        /// Away.
        /// </summary>
        Away,           // a

        /// <summary>
        /// Deaf.
        /// </summary>
        Deaf,           // d

        /// <summary>
        /// Hidden host.
        /// </summary>
        HiddenHost,     // x(undernet)

        /// <summary>
        /// Invisible.
        /// </summary>
        Invisible,      // i

        /// <summary>
        /// Operator.
        /// </summary>
        Operator,       // o

        /// <summary>
        /// Operator local.
        /// </summary>
        OperatorLocal,  // 0

        /// <summary>
        /// Restricted.
        /// </summary>
        Restricted,     // r(!undernet)

        /// <summary>
        /// Server notices.
        /// </summary>
        ServerNotices,  // s

        /// <summary>
        /// Wallops.
        /// </summary>
        Wallops,        // w
    }
}
