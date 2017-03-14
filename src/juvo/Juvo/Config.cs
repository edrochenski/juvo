using System;
using System.Collections.Generic;

/* Hierarchy of config file
 * ========================
 * 
 *  Configuration File (root)
 *      |
 *      |-- IrcConfig ("irc")
 *      |       |
 *      |       |-- Fields...
 *      |       |
 *      |       |-- IrcConfigServer[] ("servers")
 *      |       |     `-- Fields...
 *      |       |
 *      |       `-- IrcConfigChannel[] ("channels")
 *      |             `-- Fields...
 *      |       
 * 
 */

namespace JuvoConsole
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
        public string Network { get; set; }
        public string Name { get; set; }
        public string Pass { get; set; }
        public string User { get; set; }
        public string UserMode { get; set; }

        public IEnumerable<IrcConfigChannel> Channels;
        public IEnumerable<IrcConfigServer> Servers;
    }

    public class IrcConfigServer
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}