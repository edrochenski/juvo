using System;
using System.Collections.Generic;
using System.Text;

namespace Juvo.Net.Irc
{
    public class ChannelModeChangedEventArgs : EventArgs
    {
        public IEnumerable<IrcChannelModeValue> Added { get; protected set; }
        public IEnumerable<IrcChannelModeValue> Removed { get; protected set; }

        public string Channel { get; set; }

        public ChannelModeChangedEventArgs(string channel,
                                           IEnumerable<IrcChannelModeValue> added, 
                                           IEnumerable<IrcChannelModeValue> removed)
        {
            Added = added;
            Removed = removed;

            Channel = channel;
        }
    }
}
