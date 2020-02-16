// <copyright file="User.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Discord user object.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the avatar hash.
        /// </summary>
        [JsonProperty(PropertyName = "avatar")]
        public string AvatarHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        [JsonProperty(PropertyName = "discriminator")]
        public string Discriminator { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user is a bot.
        /// </summary>
        [JsonProperty(PropertyName = "bot")]
        public bool IsABot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has MFA enabled.
        /// </summary>
        [JsonProperty(PropertyName = "mfa_enabled")]
        public bool IsMfaEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email has been verified.
        /// </summary>
        [JsonProperty(PropertyName = "verified")]
        public bool IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; } = string.Empty;
    }
}
