// <copyright file="SlackMessage.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Slack
{
    /// <summary>
    /// Slack message.
    /// </summary>
    public struct SlackMessage
    {
        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        public string Team { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the Ts.
        /// </summary>
        public string Ts { get; set; }

        /// <summary>
        /// Gets or sets the sub type.
        /// </summary>
        public string SubType { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }
    }
}
