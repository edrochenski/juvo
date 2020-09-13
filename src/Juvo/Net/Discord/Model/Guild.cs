// <copyright file="Guild.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Guild object/resource. A collection of users and
    /// channels often referred to as "servers" in the UI.
    /// </summary>
    /// <remarks>
    /// <see href="https://discordapp.com/developers/docs/resources/guild">More info</see>.
    /// </remarks>
    public class Guild
    {
        /// <summary>
        /// Gets or sets the AFK channel ID.
        /// </summary>
        [JsonProperty(PropertyName = "afk_channel_id")]
        public string AfkChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the AFK timeout in seconds.
        /// </summary>
        [JsonProperty(PropertyName = "afk_timeout")]
        public int AfkTimeout { get; set; }

        /// <summary>
        /// Gets or sets the application ID.
        /// </summary>
        [JsonProperty(PropertyName = "application_id")]
        public string ApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default message notification level.
        /// </summary>
        [JsonProperty(PropertyName = "default_message_notifications")]
        public MessageNotificationLevel DefaultMessageNotifications { get; set; }

        /// <summary>
        /// Gets or sets ID of the embeddable channel.
        /// </summary>
        [JsonProperty(PropertyName = "embed_channel_id")]
        public string EmbedChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets if the guild is embeddable (i.e. widget).
        /// </summary>
        [JsonProperty(PropertyName = "embed_enabled")]
        public bool? EmbedEnabled { get; set; }

        /// <summary>
        /// Gets or sets the custom guild emojis.
        /// </summary>
        public IEnumerable<Emoji>? Emojis { get; set; }

        /// <summary>
        /// Gets or sets the explicit content filter level.
        /// </summary>
        [JsonProperty(PropertyName = "explicit_content_filter")]
        public ExplicitContentFilterLevel ExplicitContentFilter { get; set; }

        /// <summary>
        /// Gets or sets the enabled guild features.
        /// </summary>
        public IEnumerable<string>? Features { get; set; }

        /// <summary>
        /// Gets or sets the icon hash.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MFA level.
        /// </summary>
        [JsonProperty(PropertyName = "mfa_level")]
        public MfaLevel MfaLevel { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <remarks>Limited to 2-100 characters.</remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets if the current user is the owner.
        /// </summary>
        /// <remarks>
        /// Have not seen this in the logs, possibly deprecated.
        /// </remarks>
        public bool? Owner { get; set; }

        /// <summary>
        /// Gets or sets the owner's ID.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total permissions for the user (not including channel overrides.)
        /// </summary>
        public int? Permissions { get; set; }

        /// <summary>
        /// Gets or sets the voice region ID.
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the roles in the guild.
        /// </summary>
        public IEnumerable<Role>? Roles { get; set; }

        /// <summary>
        /// Gets or sets the splash hash.
        /// </summary>
        /// <remarks>
        /// Discord uses IDs/hashes to render images to the client.
        /// </remarks>
        public string Splash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the system channel ID.
        /// </summary>
        [JsonProperty(PropertyName = "system_channel_id")]
        public string SystemChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the verification level.
        /// </summary>
        [JsonProperty(PropertyName = "verification_level")]
        public VerificationLevel VerificationLevel { get; set; }

        /// <summary>
        /// Gets or sets the widget channel ID.
        /// </summary>
        [JsonProperty(PropertyName = "widget_channel_id")]
        public string WidgetChannelId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the widget is enabled.
        /// </summary>
        [JsonProperty(PropertyName = "widget_enabled")]
        public bool? WidgetEnabled { get; set; }
    }
}
