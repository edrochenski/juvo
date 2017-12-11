// <copyright file="SlackChannelPurpose.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Slack
{
    using System;

    /// <summary>
    /// Slack channel purpose.
    /// </summary>
    public struct SlackChannelPurpose
    {
        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// Gets or sets the date/time it was last set.
        /// </summary>
        public DateTime LastSet { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}
