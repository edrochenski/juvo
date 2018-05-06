// <copyright file="SocketProxy.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System.Net.Sockets;

    /// <summary>
    /// Represents a socket proxy.
    /// </summary>
    public class SocketProxy : Socket, ISocket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketProxy"/> class.
        /// </summary>
        public SocketProxy()
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
        }
    }
}
