// <copyright file="JuvoConfigScript.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Configuration
{
    /// <summary>
    /// Script configuration for Juvo.
    /// </summary>
    public class JuvoConfigScript
    {
        /// <summary>
        /// Gets or sets a value indicating whether the script is enabled in the configuration.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the filename of the script (may or may not contain the extension.)
        /// </summary>
        public string Script { get; set; }
    }
}
