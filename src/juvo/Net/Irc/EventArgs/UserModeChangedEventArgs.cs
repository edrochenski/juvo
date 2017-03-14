using System;
using System.Collections.Generic;

namespace Juvo.Net.Irc
{
    public class UserModeChangedEventArgs : EventArgs
    {
        public IEnumerable<UserMode> Added { get; private set; }
        public IEnumerable<UserMode> Removed { get; private set; }

        public UserModeChangedEventArgs(IEnumerable<UserMode> added, IEnumerable<UserMode> removed)
        {
            Added = added;
            Removed = removed;
        }
    }
}
