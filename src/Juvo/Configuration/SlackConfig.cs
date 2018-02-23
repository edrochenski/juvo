// <copyright file="SlackConfig.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Slack configuration.
    /// </summary>
    public class SlackConfig
    {
        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public IEnumerable<SlackConfigConnection> Connections { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any Slack connections should be enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
