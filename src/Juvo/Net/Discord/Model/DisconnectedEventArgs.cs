// <copyright file="DisconnectedEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Event data associated with the <see cref="DiscordClient.Disconnected"/> event.
    /// </summary>
    public class DisconnectedEventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the disconnection was user-initiated.
        /// </summary>
        public bool UserInitiated { get; set; }
    }
}
