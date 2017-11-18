// <copyright file="DiscordClientOptions.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord
{
    using System;

    /// <summary>
    /// Discord client options.
    /// </summary>
    public class DiscordClientOptions
    {
        /// <summary>
        /// Gets or sets the API URL.
        /// </summary>
        public Uri ApiUri { get; set; }

        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        public int ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the authentication token.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it is a bot.
        /// </summary>
        public bool IsBot { get; set; }

        /// <summary>
        /// Gets or sets the gateway shard.
        /// </summary>
        public int GatewayShards { get; set; }

        /// <summary>
        /// Gets or sets the gateway URI.
        /// </summary>
        public Uri GatewayUri { get; set; }
    }
}