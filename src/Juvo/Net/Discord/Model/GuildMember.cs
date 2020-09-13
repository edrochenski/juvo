// <copyright file="GuildMember.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a guild member object.
    /// </summary>
    public class GuildMember
    {
        /// <summary>
        /// Gets or sets when the user joined the guild.
        /// </summary>
        [JsonProperty(PropertyName = "joined_at")]
        public DateTime JoinedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is deaf.
        /// </summary>
        [JsonProperty(PropertyName = "deaf")]
        public bool IsDeafened { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is mute.
        /// </summary>
        [JsonProperty(PropertyName = "mute")]
        public bool IsMuted { get; set; }

        /// <summary>
        /// Gets or sets the nick.
        /// </summary>
        public string Nick { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public IEnumerable<string>? Roles { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public User? User { get; set; }
    }
}
