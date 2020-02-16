// <copyright file="StrideConfigConnection.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    /// <summary>
    /// Represents a Stride connection.
    /// </summary>
    public class StrideConfigConnection
    {
        /// <summary>
        /// Gets or sets the client ID.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether Stride is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
