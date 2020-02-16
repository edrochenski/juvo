// <copyright file="Activity.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Represents a Game.
    /// </summary>
    public class Activity
    {
        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Type.
        /// </summary>
        public GameType Type { get; set; }

        /// <summary>
        /// Gets or sets Url.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
