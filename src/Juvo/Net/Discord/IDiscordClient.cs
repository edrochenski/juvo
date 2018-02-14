// <copyright file="IDiscordClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Starts the connection process.
    /// </summary>
    /// <returns>A Task associated with the async operation.</returns>
    public interface IDiscordClient : IDisposable
    {
        /// <summary>
        /// Fires when a ready event is received.
        /// </summary>
        event ReadyReceivedEventHandler ReadyReceived;

        /// <summary>
        /// Connects to discord.
        /// </summary>
        /// <returns>A task.</returns>
        Task Connect();
    }
}
