// <copyright file="DiscordConfigConnection.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    /// <summary>
    /// Discord configuration connection.
    /// </summary>
    public class DiscordConfigConnection
    {
        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        public string? AuthToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
