// <copyright file="ISlackClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Slack
{
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;

    /// <summary>
    /// Represents a slack client.
    /// </summary>
    public interface ISlackClient
    {
        /// <summary>
        /// Event raised when a message is received from the server.
        /// </summary>
        event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// Event raised when a user's presence changes.
        /// </summary>
        event PresenceChangedEventHandler PresenceChanged;

        /// <summary>
        /// Event raised when a user types.
        /// </summary>
        event UserTypingEventHandler UserTyping;

        /// <summary>
        /// Connects to the remote server aynchronously.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task Connect();

        /// <summary>
        /// Disconnects from the remote server.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task Disconnect();

        /// <summary>
        /// Intializes the client.
        /// </summary>
        /// <param name="token">Token.</param>
        void Initialize(string token);

        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="channel">Channel to post the message to.</param>
        /// <param name="text">Text of the message.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendMessage(string channel, string text);
    }
}
