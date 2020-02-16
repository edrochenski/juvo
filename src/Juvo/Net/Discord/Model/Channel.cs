// <copyright file="Channel.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Base channel object.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Gets or sets the Application ID.
        /// </summary>
        [JsonProperty(PropertyName = "application_id")]
        public string ApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the bitrate.
        /// </summary>
        [JsonProperty(PropertyName = "bitrate")]
        public int? Bitrate { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Guild ID.
        /// </summary>
        [JsonProperty(PropertyName = "guild_id")]
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Icon Hash.
        /// </summary>
        [JsonProperty(PropertyName = "icon")]
        public string IconHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last message ID.
        /// </summary>
        [JsonProperty(PropertyName = "last_message_id")]
        public string LastMessageId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last last pin date/time.
        /// </summary>
        [JsonProperty(PropertyName = "last_pin_timestamp")]
        public string LastPin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether channel is NSFW.
        /// </summary>
        [JsonProperty(PropertyName = "nsfw")]
        public bool Nsfw { get; set; }

        /// <summary>
        /// Gets or sets the Owner ID.
        /// </summary>
        [JsonProperty(PropertyName = "owner_id")]
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Parent ID.
        /// </summary>
        [JsonProperty(PropertyName = "parent_id")]
        public string ParentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        [JsonProperty(PropertyName = "position")]
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        [JsonProperty(PropertyName = "recipients")]
        public IEnumerable<User>? Recipients { get; set; }

        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        [JsonProperty(PropertyName = "topic")]
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets type of channel.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public ChannelType Type { get; set; }

        /// <summary>
        /// Gets or sets the user limit.
        /// </summary>
        [JsonProperty(PropertyName = "user_limit")]
        public int? UserLimit { get; set; }
    }
}
