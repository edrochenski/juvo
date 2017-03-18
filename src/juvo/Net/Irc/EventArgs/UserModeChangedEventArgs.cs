using System;
using System.Collections.Generic;

namespace Juvo.Net.Irc
{
    public class UserModeChangedEventArgs : EventArgs
    {
        public IEnumerable<IrcUserMode> Added { get; protected set; }
        public IEnumerable<IrcUserMode> Removed { get; protected set; }

        public UserModeChangedEventArgs(IEnumerable<IrcUserMode> added, IEnumerable<IrcUserMode> removed)
        {
            Added = added;
            Removed = removed;
        }
    }
}
