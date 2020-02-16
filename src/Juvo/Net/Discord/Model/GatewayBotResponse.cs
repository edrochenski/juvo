// <copyright file="GatewayBotResponse.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Gateway bot response.
    /// </summary>
    public class GatewayBotResponse
    {
        /// <summary>
        /// Gets or sets the shards.
        /// </summary>
        public int Shards { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
