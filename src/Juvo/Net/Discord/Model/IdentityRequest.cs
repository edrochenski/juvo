// <copyright file="IdentityRequest.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Game type
    /// </summary>
    public enum GameType
    {
        /// <summary>
        /// Game.
        /// </summary>
        Game = 0,

        /// <summary>
        /// Streaming.
        /// </summary>
        Streaming = 1
    }

    /// <summary>
    /// Identity request.
    /// </summary>
    public class IdentityRequest : GatewayPayload
    {
        /// <summary>
        /// Gets or sets the Data.
        /// </summary>
        [JsonProperty(PropertyName ="d")]
        public new IdentityData? Data { get; set; }

        /// <summary>
        /// Gets or sets the shards.
        /// </summary>
#pragma warning disable SA1011 // Closing square brackets must be spaced correctly
        [JsonProperty(PropertyName = "shard")]
        public int[]? Shard { get; set; }
#pragma warning restore SA1011 // Closing square brackets must be spaced correctly

        /// <summary>
        /// Representation
        /// </summary>
        public class IdentityData
        {
            /// <summary>
            /// Gets or sets a value indicating whether compression is supported.
            /// </summary>
            [JsonProperty(PropertyName = "compress")]
            public bool Compress { get; set; }

            /// <summary>
            /// Gets or sets the large threshold.
            /// </summary>
            [JsonProperty(PropertyName = "large_threshold")]
            public int LargeThreshold { get; set; }

            /// <summary>
            /// Gets or sets the presence.
            /// </summary>
            [JsonProperty(PropertyName = "presence")]
            public StatusUpdate? Presence { get; set; }

            /// <summary>
            /// Gets or sets the properties.
            /// </summary>
            [JsonProperty(PropertyName = "properties")]
            public ConnectionProperties? Properties { get; set; }

            /// <summary>
            /// Gets or sets the token.
            /// </summary>
            [JsonProperty(PropertyName = "token")]
            public string Token { get; set; } = string.Empty;
        }

        /// <summary>
        /// Connection properties.
        /// </summary>
        public class ConnectionProperties
        {
            /// <summary>
            /// Gets or sets the browser.
            /// </summary>
            [JsonProperty(PropertyName = "$browser")]
            public string Browser { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the device.
            /// </summary>
            [JsonProperty(PropertyName = "$device")]
            public string Device { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the OS.
            /// </summary>
            [JsonProperty(PropertyName = "$os")]
            public string Os { get; set; } = string.Empty;
        }

        /// <summary>
        /// Game object.
        /// </summary>
        public class GameObject
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            [JsonProperty(PropertyName = "type")]
            public GameType Type { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            [JsonProperty(PropertyName = "url")]
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Status update.
        /// </summary>
        public class StatusUpdate
        {
            /// <summary>
            /// Gets or sets a value indicating whether AFK.
            /// </summary>
            [JsonProperty(PropertyName = "afk")]
            public bool Afk { get; set; }

            /// <summary>
            /// Gets or sets the game.
            /// </summary>
            [JsonProperty(PropertyName = "game")]
            public GameObject? Game { get; set; }

            /// <summary>
            /// Gets or sets since.
            /// </summary>
            [JsonProperty(PropertyName = "since")]
            public int? Since { get; set; }

            /// <summary>
            /// Gets or sets status.
            /// </summary>
            [JsonProperty(PropertyName = "status")]
            public string Status { get; set; } = string.Empty;
        }
    }
}
