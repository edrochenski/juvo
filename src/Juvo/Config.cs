using System;
using System.Collections.Generic;

namespace Juvo
{
    public class Config
    {
        public IrcConfig Irc;
    }

    public class IrcConfig
    {
        public IEnumerable<IrcConfigConnection> Connections { get; set; }
        public string Nickname { get; set; }
        public string RealName { get; set; }
        public string Username { get; set; }
    }

    public class IrcConfigChannel
    {
        public string Name { get; set; }
    }

    public class IrcConfigConnection
    {
        public string Name { get; set; }

        public IEnumerable<IrcConfigChannel> Channels;
        public IEnumerable<IrcConfigServer> Servers;
    }

    public class IrcConfigServer
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}