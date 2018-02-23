// <copyright file="JuvoConfig.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// Juvo configuration.
    /// </summary>
    public class JuvoConfig
    {
        /// <summary>
        /// Gets or sets the scripts from the configuration file.
        /// </summary>
        public IEnumerable<JuvoConfigScript> Scripts { get; set; }
    }
}
