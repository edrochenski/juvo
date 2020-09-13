// <copyright file="Role.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a Discord role object.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the color (int representation of the hexadecimal value.)
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role pinned in the user listing.
        /// </summary>
        [JsonProperty(PropertyName = "hoist")]
        public bool Hoisted { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the role is managed.
        /// </summary>
        public bool Managed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the role is mentionable.
        /// </summary>
        public bool Mentionable { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the permissions (bit flags.)
        /// </summary>
        public int Permissions { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public int Position { get; set; }
    }
}
