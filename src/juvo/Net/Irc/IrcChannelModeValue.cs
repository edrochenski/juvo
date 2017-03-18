using System;
using System.Collections.Generic;
using System.Text;

namespace Juvo.Net.Irc
{
    public class IrcChannelModeValue
    {
        public IrcChannelMode Mode { get; set; }
        public string Value { get; set; }

        public IrcChannelModeValue(IrcChannelMode mode, string value)
        {
            Mode = mode;
            Value = value;
        }
    }
}
