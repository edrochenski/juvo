// <copyright file="ISocketClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;

    /// <summary>
    /// Represents a SocketClient.
    /// </summary>
    public interface ISocketClient
    {
        /// <summary>
        /// Fires when a connection has completed successfully.
        /// </summary>
        event EventHandler<EventArgs> ConnectCompleted;

        /// <summary>
        /// Fires when a connection attempt has failed.
        /// </summary>
        event EventHandler<EventArgs> ConnectFailed;

        /// <summary>
        /// Fires when the client is disconnected.
        /// </summary>
        event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Fires when the current receive process is completed.
        /// </summary>
        event EventHandler<ReceiveCompletedEventArgs> ReceiveCompleted;

        /// <summary>
        /// Fires when the current receive process fails.
        /// </summary>
        event EventHandler<SocketEventArgs> ReceiveFailed;

        /// <summary>
        /// Fires when a send is completed successfully.
        /// </summary>
        event EventHandler<EventArgs> SendCompleted;

        /// <summary>
        /// Fires when a send fails to start or complete.
        /// </summary>
        event EventHandler<SocketEventArgs> SendFailed;

        /// <summary>
        /// Initiates the connection process to the remote endpoint.
        /// </summary>
        /// <param name="host">Host to connect to.</param>
        /// <param name="port">Port to connect to.</param>
        void Connect(string host, int port);

        /// <summary>
        /// Send text to the remote endpoint.
        /// </summary>
        /// <param name="text">Text to send.</param>
        void Send(string text);

        /// <summary>
        /// Send bytes to the remote endpoint.
        /// </summary>
        /// <param name="bytes">Bytes to send.</param>
        void Send(byte[] bytes);

        /// <summary>
        /// Dispose.
        /// </summary>
        void Dispose();
    }
}
