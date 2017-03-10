using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public class DataReceivedArgs : EventArgs
    {
        byte[] data;
        public byte[] Data { get { return data; } }

        public DataReceivedArgs(byte[] data)
        { this.data = data; }
    }
}
