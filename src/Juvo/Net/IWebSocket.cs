// <copyright file="IWebSocket.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a way for connecting to a websocket endpoint.
    /// </summary>
    public interface IWebSocket : IDisposable
    {
        /*/ Properties /*/

        /// <summary>
        /// Gets the reason why the remote endpoint initiated the close handshake.
        /// </summary>
        WebSocketCloseStatus? CloseStatus { get; }

        /// <summary>
        /// Gets the remote endpoint's description of the reason why the connection was closed.
        /// </summary>
        string? CloseStatusDescription { get; }

        /// <summary>
        /// Gets the current state of the WebSocket connection.
        /// </summary>
        WebSocketState State { get; }

        /// <summary>
        /// Gets the subprotocol that was negotiated during the opening handshake.
        /// </summary>
        string? SubProtocol { get; }

        /*/ Methods /*/

        /// <summary>
        /// Aborts the WebSocket connection and cancels any pending IO operations.
        /// </summary>
        void Abort();

        /// <summary>
        /// Closes the WebSocket connection as an asynchronous operation using the close handshake defined in the http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-06 section 7.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the WebSocket connection.</param>
        /// <param name="statusDescription">Specifies a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// Initiates or completes the close handshake defined in the http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-06.
        /// </summary>
        /// <param name="closeStatus">Indicates the reason for closing the WebSocket connection.</param>
        /// <param name="statusDescription">Allows applications to specify a human readable explanation as to why the connection is closed.</param>
        /// <param name="cancellationToken">The token that can be used to propagate notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken);

        /// <summary>
        /// Receives data from the System.Net.WebSockets.WebSocket connection asynchronously.
        /// </summary>
        /// <param name="buffer">References the application buffer that is the storage location for the received data.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation. The System.Threading.Tasks.Task`1.Result property on the task object returns a System.Byte array containing the received data.</returns>
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// Sends data over the System.Net.WebSockets.WebSocket connection asynchronously.
        /// </summary>
        /// <param name="buffer">The buffer to be sent over the connection.</param>
        /// <param name="messageType">Indicates whether the application is sending a binary or text message.</param>
        /// <param name="endOfMessage">Indicates whether the data in "buffer" is the last part of a message.</param>
        /// <param name="cancellationToken">The token that propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    }
}
