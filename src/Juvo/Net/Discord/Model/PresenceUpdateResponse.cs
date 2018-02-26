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
        public new PresenceUpdateData Data { get; set; }

        /// <summary>
        /// Represents the data in a Presence Update response.
        /// </summary>
        public class PresenceUpdateData
        {
            /// <summary>
            /// Gets or sets the Game.
            /// </summary>
            public Game Game { get; set; }

            /// <summary>
            /// Gets or sets the guild ID.
            /// </summary>
            [JsonProperty(PropertyName = "guild_id")]
            public string GuildId { get; set; }

            /// <summary>
            /// Gets or sets nick, if any.
            /// </summary>
            public string Nick { get; set; }

            /// <summary>
            /// Gets or sets nick, if any.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Gets or sets roles.
            /// </summary>
            public IEnumerable<string> Roles { get; set; }

            /// <summary>
            /// Gets or sets the user.
            /// </summary>
            public User User { get; set; }
        }
    }
}
