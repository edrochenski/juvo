// <copyright file="StrideConfig.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Stride configuration.
    /// </summary>
    public class StrideConfig
    {
        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public IEnumerable<StrideConfigConnection> Connections { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Stride is enabled.
        /// </summary>
        public bool Enabled { get; set; }
    }
}
