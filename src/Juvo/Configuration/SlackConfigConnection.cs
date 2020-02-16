// <copyright file="SlackConfigConnection.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    /// <summary>
    /// Slack configuration connection.
    /// </summary>
    public class SlackConfigConnection
    {
        /// <summary>
        /// Gets or sets the command token.
        /// </summary>
        public string CommandToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the connection is enabled.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}
