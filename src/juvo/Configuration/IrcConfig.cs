// <copyright file="IrcConfig.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// IRC configuration.
    /// </summary>
    public class IrcConfig
    {
        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public IEnumerable<IrcConfigConnection> Connections { get; set; }
    }
}
