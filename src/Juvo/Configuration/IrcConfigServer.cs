// <copyright file="IrcConfigServer.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    /// <summary>
    /// IRC configuration server.
    /// </summary>
    public class IrcConfigServer
    {
        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public int Port { get; set; }
    }
}
