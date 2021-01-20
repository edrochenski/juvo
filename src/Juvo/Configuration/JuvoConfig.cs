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
        /// Gets or sets the base path for the bot. Typically where
        /// the .config/.dat/etc data will reside.
        /// </summary>
        public string? BasePath { get; set; }

        /// <summary>
        /// Gets or sets the data path for the bot. This is where logs,
        /// temporary data, etc will reside.
        /// </summary>
        public string? DataPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bot should stop/shutdown
        /// if it encounters errors while compiling scripts.
        /// </summary>
        public bool StopOnCompileErrors { get; set; }

        /// <summary>
        /// Gets or sets the scripts from the configuration file.
        /// </summary>
        public IEnumerable<JuvoConfigScript>? Scripts { get; set; }
    }
}
