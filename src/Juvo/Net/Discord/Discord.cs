// <copyright file="Discord.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord
{
    /// <summary>
    /// Container for Discord-related values.
    /// </summary>
    public static class Discord
    {
        /// <summary>
        /// Default URL of the Discord API.
        /// </summary>
        public const string DefaultApiUrl = "https://discordapp.com/api";

        /// <summary>
        /// Default version of the Discord API.
        /// </summary>
        public const int DefaultApiVersion = 6;

        /// <summary>
        /// Default encoding of the Discord API.
        /// </summary>
        public const string DefaultApiEncoding = "json";

        /// <summary>
        /// Default heartbeat interval.
        /// </summary>
        public const int DefaultHeartbeatInterval = 60000;

        /// <summary>
        /// Container for Discord-related API paths.
        /// </summary>
        public class ApiPaths
        {
            /// <summary>
            /// Path to the gateway endpoint.
            /// </summary>
            public const string Gateway = "gateway";

            /// <summary>
            /// Path to the gateway/bot endpoint.
            /// </summary>
            public const string GatewayBot = "gateway/bot";
        }
    }
}
