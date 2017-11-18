// <copyright file="SlackUserTyping.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace BytedownSoftware.Lib.Net.Slack
{
    /// <summary>
    /// Slack user typing.
    /// </summary>
    public struct SlackUserTyping
    {
        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }
    }
}
