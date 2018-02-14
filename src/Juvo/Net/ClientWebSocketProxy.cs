// <copyright file="ClientWebSocketProxy.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy ClientWebSocket for implementing IClientWebSocket.
    /// </summary>
    public class ClientWebSocketProxy : IClientWebSocket
    {
/*/ Fields /*/
        private readonly ClientWebSocket webSocket;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientWebSocketProxy"/> class.
        /// </summary>
        public ClientWebSocketProxy()
        {
            this.webSocket = new ClientWebSocket();
        }

/*/ Properties /*/

        /// <summary>
        /// Gets the reason the Close handshake was initiated.
        /// </summary>
        public WebSocketCloseStatus? CloseStatus => this.webSocket.CloseStatus;

        /// <summary>
        /// Gets a description of the reason why the instance was closed.
        /// </summary>
        public string CloseStatusDescription => this.webSocket.CloseStatusDescription;

        /// <summary>
        /// Gets the WebSocket options for the instance.
        /// </summary>
        public ClientWebSocketOptions Options => this.webSocket.Options;

        /// <summary>
        /// Gets the WebSocket state of the instance.
        /// </summary>
        public WebSocketState State => this.webSocket.State;

        /// <summary>
        /// Gets the supported WebSocket sub-protocol for the instance.
        /// </summary>
        public string SubProtocol => this.webSocket.SubProtocol;

/*/ Methods /*/

        /// <summary>
        /// Aborts the connection and aborts any pending IO operations.
        /// </summary>
        public void Abort() => this.webSocket.Abort();

        /// <summary>
        /// Close the instance as an asynchronous operation.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription">A description of the close status.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task CloseAsync(
            WebSocketCloseStatus closeStatus,
            string statusDescription,
            CancellationToken cancellationToken)
            => await this.webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        /// <summary>
        /// Close the output for the instance as an asynchronous operation.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription">A description of the close status.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task CloseOutputAsync(
            WebSocketCloseStatus closeStatus,
            string statusDescription,
            CancellationToken cancellationToken)
            => await this.webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);

        /// <summary>
        /// Connect to a WebSocket server as an asynchronous operation.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server to connect to.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
            => await this.webSocket.ConnectAsync(uri, cancellationToken);

        /// <summary>
        /// Dispose of any instance resources.
        /// </summary>
        public void Dispose() => this.webSocket?.Dispose();

        /// <summary>
        /// Receive data as an async operation.
        /// </summary>
        /// <param name="buffer">Buffer to receive the response.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation with the result of operation.</returns>
        public async Task<WebSocketReceiveResult> ReceiveAsync(
            ArraySegment<byte> buffer,
            CancellationToken cancellationToken)
            => await this.webSocket.ReceiveAsync(buffer, cancellationToken);

        /// <summary>
        /// Sends data as an async operation.
        /// </summary>
        /// <param name="buffer">Buffer to receive the response.</param>
        /// <param name="messageType">Specifies if the message is clear text or binary.</param>
        /// <param name="endOfMessage">Specifies if this is the final async send.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendAsync(
            ArraySegment<byte> buffer,
            WebSocketMessageType messageType,
            bool endOfMessage,
            CancellationToken cancellationToken)
            => await this.webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
    }
}
