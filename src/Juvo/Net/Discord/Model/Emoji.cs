// <copyright file="Emoji.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an emoji.
    /// </summary>
    public class Emoji
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the emoji is animated.
        /// </summary>
        public bool IsAnimated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the emoji is managed.
        /// </summary>
        public bool IsManaged { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the emoji must be wrapped in colons.
        /// </summary>
        [JsonProperty(PropertyName = "require_colons")]
        public bool RequiresColons { get; set; }

        /// <summary>
        /// Gets or sets the role this emoji is whitelisted to.
        /// </summary>
        public IEnumerable<string>? Roles { get; set; }

        /// <summary>
        /// Gets or sets the user who created this emoji.
        /// </summary>
        public User? User { get; set; }
    }
}
