// <copyright file="DiscordConfig.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Discord config.
    /// </summary>
    public class DiscordConfig
    {
        /// <summary>
        /// Gets or sets Discord connections.
        /// </summary>
        public IEnumerable<DiscordConfigConnection>? Connections { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any Discord connections should be enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
