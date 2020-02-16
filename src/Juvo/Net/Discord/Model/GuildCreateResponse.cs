// <copyright file="GuildCreateResponse.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Guild create response.
    /// </summary>
    public class GuildCreateResponse : GatewayPayload
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public new GuildCreateData? Data { get; set; }

        /// <summary>
        /// Represents the data in a GUILD_CREATE response.
        /// </summary>
        public class GuildCreateData : Guild
        {
            /// <summary>
            /// Gets or sets the channels.
            /// </summary>
            public IEnumerable<Channel>? Channels { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the guild is considered large.
            /// </summary>
            public bool? IsLarge { get; set; }

            /// <summary>
            /// Gets or sets when the guild was joined.
            /// </summary>
            [JsonProperty(PropertyName = "joined_at")]
            public DateTime? JoinedAt { get; set; }

            /// <summary>
            /// Gets or sets the total number of members in the guild.
            /// </summary>
            [JsonProperty(PropertyName = "member_count")]
            public int? MemberCount { get; set; }

            /// <summary>
            /// Gets or sets the members.
            /// </summary>
            public IEnumerable<GuildMember>? Members { get; set; }

            /// <summary>
            /// Gets or sets pesences.
            /// </summary>
            public IEnumerable<PresenceUpdate>? Presences { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the guild is unavailable.
            /// </summary>
            public bool? Unavailable { get; set; }

            /// <summary>
            /// Gets or sets the current (partial) voice state objects.
            /// </summary>
            /// <remarks>
            /// Voice state will not contain the guild_id key.
            /// </remarks>
            [JsonProperty(PropertyName = "voice_states")]
            public IEnumerable<VoiceState>? VoiceStates { get; set; }
        }
    }
}
