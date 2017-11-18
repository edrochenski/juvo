// <copyright file="GatewayOpCode.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Gateway op code.
    /// </summary>
    public enum GatewayOpCode
    {
        /// <summary>
        /// Dispatches an event (Receive)
        /// </summary>
        Dispatch = 0,

        /// <summary>
        /// Used for ping checking (Send/Receive)
        /// </summary>
        Heartbeat = 1,

        /// <summary>
        /// Used for client handshake (Send)
        /// </summary>
        Identity = 2,

        /// <summary>
        /// Used to update the client status (Send)
        /// </summary>
        StatusUpdate = 3,

        /// <summary>
        /// Used to join/move/leave voice channels (Send)
        /// </summary>
        VoiceStateUpdate = 4,

        /// <summary>
        /// Used for voice ping checking (Send)
        /// </summary>
        VoiceServerPing = 5,

        /// <summary>
        /// Used to resume a closed connection (Send)
        /// </summary>
        Resume = 6,

        /// <summary>
        /// Used to tell clients to reconnect to the gateway (Receive)
        /// </summary>
        Reconnect = 7,

        /// <summary>
        /// Used to request guild members (Send)
        /// </summary>
        RequestGuildMembers = 8,

        /// <summary>
        /// Used to notify client they have an invalid session id (Receive)
        /// </summary>
        InvalidSession = 9,

        /// <summary>
        /// Sent immediately after connecting, contains heartbeat and server debug information (Receive)
        /// </summary>
        Hello = 10,

        /// <summary>
        /// Sent immediately following a client heartbeat that was received (Receive)
        /// </summary>
        HeartbeatAck = 11
    }
}
