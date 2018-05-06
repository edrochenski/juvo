// <copyright file="ISocket.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Represents a socket.
    /// </summary>
    public interface ISocket : IDisposable
    {
        /// <summary>
        /// Begins an asynchronous request for a connection to a remote host.
        /// </summary>
        /// <param name="e">
        /// The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.
        /// </param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The System.Net.Sockets.SocketAsyncEventArgs. Completed
        /// event on the e parameter will be raised upon completion of the operation. Returns false if the I/O
        /// operation completed synchronously. In this case, The System.Net.Sockets.SocketAsyncEventArgs. Completed
        /// event on the e parameter will not be raised and the e object passed as a parameter may be examined
        /// immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool ConnectAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// Begins an asynchronous request to receive data from a connected System.Net.Sockets.Socket object.
        /// </summary>
        /// <param name="e">
        /// The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.
        /// </param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The System.Net.Sockets.SocketAsyncEventArgs. Completed
        /// event on the e parameter will be raised upon completion of the operation. Returns false if the I/O
        /// operation completed synchronously. In this case, The System.Net.Sockets.SocketAsyncEventArgs. Completed
        /// event on the e parameter will not be raised and the e object passed as a parameter may be examined
        /// immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool ReceiveAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// Sends data asynchronously to a connected System.Net.Sockets.Socket object.
        /// </summary>
        /// <param name="e">
        /// The System.Net.Sockets.SocketAsyncEventArgs object to use for this asynchronous socket operation.
        /// </param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The System.Net.Sockets.SocketAsyncEventArgs.Completed
        /// event on the e parameter will be raised upon completion of the operation. Returns false if the I/O
        /// operation completed synchronously. In this case, The System.Net.Sockets.SocketAsyncEventArgs. Completed
        /// event on the e parameter will not be raised and the e object passed as a parameter may be examined
        /// immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        bool SendAsync(SocketAsyncEventArgs e);

        /// <summary>
        /// Disables sends and receives on a System.Net.Sockets.Socket.
        /// </summary>
        /// <param name="how">
        /// One of the System.Net.Sockets.SocketShutdown values that specifies the operation that will no
        /// longer be allowed.
        /// </param>
        void Shutdown(SocketShutdown how);
    }
}
