// <copyright file="IClientWebSocket.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a client for connecting to a websocket.
    /// </summary>
    public interface IClientWebSocket : IWebSocket
    {
/*/ Properties /*/

        /// <summary>
        /// Gets the optons for the <see cref="ClientWebSocket"/>.
        /// </summary>
        ClientWebSocketOptions Options { get; }

        /// <summary>
        /// Connects to the websocket asynchronously.
        /// </summary>
        /// <param name="uri">URI of the websocket service.</param>
        /// <param name="cancellationToken">Propagates the notification that operations should be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
    }
}
