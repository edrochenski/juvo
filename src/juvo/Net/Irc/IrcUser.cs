using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public class IrcUser
    {
        string _host;
        string _nickname;
        string _username;

        public string Host
        {
            get { return _host; }
        }
        public string Nickname
        {
            get { return _nickname; }
        }
        public string Username
        {
            get { return _username; }
        }

        public IrcUser(string identifier)
        {
            if (identifier.Contains("!") && identifier.Contains("@"))
            {
                string[] parts = identifier.Split('@');
                string[] userParts = parts[0].Split('!');
                _host = parts[1];
                _nickname = userParts[0];
                _username = userParts[1];
            }
            else
            {
                _nickname = identifier;
            }
        }
    }
}
