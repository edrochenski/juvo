// <copyright file="GatewayPayload.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Gateway payload.
    /// </summary>
    public class GatewayPayload
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        [JsonProperty(PropertyName = "t")]
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the op code.
        /// </summary>
        [JsonProperty(PropertyName = "op")]
        public GatewayOpCode OpCode { get; set; }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        [JsonProperty(PropertyName = "s")]
        public int? Sequence { get; set; }
    }
}
