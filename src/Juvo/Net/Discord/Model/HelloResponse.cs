// <copyright file="HelloResponse.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Hello response.
    /// </summary>
    public class HelloResponse : GatewayPayload
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public new HelloData? Data { get; set; }

        /// <summary>
        /// Hello data.
        /// </summary>
        public class HelloData
        {
            /// <summary>
            /// Gets or sets the Heartbeat Interval.
            /// </summary>
            [JsonProperty(PropertyName = "heartbeat_interval")]
            public int HeartbeatInterval { get; set; }

            /// <summary>
            /// Gets or sets trace.
            /// </summary>
            [JsonProperty(PropertyName = "_trace")]
            public IEnumerable<string>? Trace { get; set; }
        }
    }
}
