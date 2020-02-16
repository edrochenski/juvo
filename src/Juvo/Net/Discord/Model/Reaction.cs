// <copyright file="Reaction.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Represents a reaction.
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// Gets or sets the number of times the emoji has been used to react.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the emoji information.
        /// </summary>
        public Emoji? Emoji { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user reacted using this emoji.
        /// </summary>
        public bool Me { get; set; }
    }
}
