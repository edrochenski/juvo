// <copyright file="SlackUser.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Slack
{
    /// <summary>
    /// Slack user.
    /// </summary>
    public struct SlackUser
    {
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 2FA.
        /// </summary>
        public bool Has2fa { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether has files.
        /// </summary>
        public bool HasFiles { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is admin.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is bot.
        /// </summary>
        public bool IsBot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is owner.
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is primary owner.
        /// </summary>
        public bool IsPrimaryOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is restricted.
        /// </summary>
        public bool IsRestricted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is ultra restricted.
        /// </summary>
        public bool IsUltraRestricted { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the presence.
        /// </summary>
        public string Presence { get; set; }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        public SlackUserProfile Profile { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the team ID.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets the TZ.
        /// </summary>
        public string Tz { get; set; }

        /// <summary>
        /// Gets or sets the TZ label.
        /// </summary>
        public string TzLabel { get; set; }

        /// <summary>
        /// Gets or sets the TZ offset.
        /// </summary>
        public int TzOffset { get; set; }
    }
}
