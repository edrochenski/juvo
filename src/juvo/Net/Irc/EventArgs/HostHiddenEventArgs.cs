using System;

namespace Juvo.Net.Irc
{
    public class HostHiddenEventArgs : EventArgs
    {
        public readonly string Host;

        public HostHiddenEventArgs(string host)
        {
            Host = host;
        }
    }
}
