// <copyright file="SlackPresenceChange.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace BytedownSoftware.Lib.Net.Slack
{
    /// <summary>
    /// Slack presence change.
    /// </summary>
    public struct SlackPresenceChange
    {
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the presence.
        /// </summary>
        public string Presence { get; set; }
    }
}
