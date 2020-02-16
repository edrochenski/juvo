// <copyright file="PresenceUpdateResponse.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Presence Update event.
    /// </summary>
    public class PresenceUpdateResponse : GatewayPayload
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public new PresenceUpdateData? Data { get; set; }

        /// <summary>
        /// Represents the data in a Presence Update response.
        /// </summary>
        public class PresenceUpdateData : PresenceUpdate
        {
        }
    }
}
