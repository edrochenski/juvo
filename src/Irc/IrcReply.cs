using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace juvo.Irc
{
    public class IrcReply
    {
        string _command;
        string[] _params;
        string _prefix;
        string _target;
        string _trailing;

        public string Command { get { return _command; } }
        public string[] Params { get { return _params; } }
        public string Prefix { get { return _prefix; } }
        public string Target { get { return _target; } }
        public bool TargetIsChannel { get { return !String.IsNullOrEmpty(_target) && IrcClient.ChannelIdents.Contains(_target[0]); } }
        public bool TargetIsUser { get { return !TargetIsChannel; } }
        public string Trailing { get { return _trailing; } }

        public IrcReply(string rawMessage)
        {
            string[] sects = rawMessage.Split(new char[] { ':' }, 3);
            // ignore sects[0] since it should just be empty

            if (sects.Length > 1)
            {
                string[] parts = sects[1].Split(' ');

                if (parts.Length > 0) { _prefix = parts[0]; }
                if (parts.Length > 1) { _command = parts[1]; }
                if (parts.Length > 2) { _target = parts[2]; }
                if (parts.Length > 3)
                {
                    _params = new string[parts.Length - 3];
                    for (int x = 3; x < parts.Length; ++x)
                    { _params[x - 3] = parts[x]; }
                }
            }

            if (sects.Length > 2)
            { _trailing = sects[2]; }
        }
    }
}
