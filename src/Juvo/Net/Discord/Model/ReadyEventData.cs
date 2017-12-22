// <copyright file="ReadyEventData.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Ready event.
    /// </summary>
    public class ReadyEventData : GatewayPayload
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public new ReadyData Data { get; set; }

        /// <summary>
        /// Ready data.
        /// </summary>
        public class ReadyData
        {
            /// <summary>
            /// Gets or sets the private channels.
            /// </summary>
            [JsonProperty(PropertyName = "private_channels")]
            public IEnumerable<Channel> PrivateChannels { get; set; }

            /// <summary>
            /// Gets or sets the gateway protocol version.
            /// </summary>
            [JsonProperty(PropertyName = "v")]
            public int ProtocolVersion { get; set; }

            /// <summary>
            /// Gets or sets the session ID.
            /// </summary>
            [JsonProperty(PropertyName = "session_id")]
            public string SessionId { get; set; }

            /// <summary>
            /// Gets or sets trace.
            /// </summary>
            [JsonProperty(PropertyName = "_trace")]
            public IEnumerable<string> Trace { get; set; }

            /// <summary>
            /// Gets or sets the user.
            /// </summary>
            [JsonProperty(PropertyName = "user")]
            public User User { get; set; }
        }
    }
}
