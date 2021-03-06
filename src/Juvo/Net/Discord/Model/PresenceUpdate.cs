﻿// <copyright file="PresenceUpdate.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents presence update.
    /// </summary>
    public class PresenceUpdate
    {
        /// <summary>
        /// Gets or sets the Game.
        /// </summary>
        public Activity? Game { get; set; }

        /// <summary>
        /// Gets or sets the guild ID.
        /// </summary>
        [JsonProperty(PropertyName = "guild_id")]
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets nick, if any.
        /// </summary>
        public string Nick { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets nick, if any.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets roles.
        /// </summary>
        public IEnumerable<string>? Roles { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public User? User { get; set; }
    }
}
