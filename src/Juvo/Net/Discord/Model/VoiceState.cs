// <copyright file="VoiceState.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a voice state.
    /// </summary>
    public class VoiceState
    {
        /// <summary>
        /// Gets or sets the channel ID the user is connected to.
        /// </summary>
        [JsonProperty(PropertyName = "channel_id")]
        public string ChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the guild ID the voice state is for.
        /// </summary>
        [JsonProperty(PropertyName = "guild_id")]
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user is deafened by the server.
        /// </summary>
        [JsonProperty(PropertyName = "deaf")]
        public bool IsDeaf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is muted by the server.
        /// </summary>
        [JsonProperty(PropertyName = "mute")]
        public bool IsMute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is locally deafened.
        /// </summary>
        [JsonProperty(PropertyName = "self_deaf")]
        public bool IsSelfDeaf { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is locally muted.
        /// </summary>
        [JsonProperty(PropertyName = "self_mute")]
        public bool IsSelfMute { get; set; }

        /// <summary>
        /// Gets or sets the guild ID the voice state is for.
        /// </summary>
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user is muted by the current user.
        /// </summary>
        [JsonProperty(PropertyName = "suppress")]
        public bool Supressed { get; set; }

        /// <summary>
        /// Gets or sets the user ID the voice state is for.
        /// </summary>
        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; } = string.Empty;
    }
}
