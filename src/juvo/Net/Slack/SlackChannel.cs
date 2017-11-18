// <copyright file="SlackChannel.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace BytedownSoftware.Lib.Net.Slack
{
    using System.Collections.Generic;

    // TODO: more descriptive xmldocs

    /// <summary>
    /// Slack channel.
    /// </summary>
    public struct SlackChannel
    {
        /// <summary>
        /// Gets or sets created unix time.
        /// </summary>
        public long Created { get; set; }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether pins are available.
        /// </summary>
        public bool HasPins { get; set; }

        /// <summary>
        /// Gets or sets ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether channel.
        /// </summary>
        public bool IsChannel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether general.
        /// </summary>
        public bool IsGeneral { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether member.
        /// </summary>
        public bool IsMember { get; set; }

        /// <summary>
        /// Gets or sets the last read.
        /// </summary>
        public string LastRead { get; set; }

        /// <summary>
        /// Gets or sets the latest.
        /// </summary>
        public string Latest { get; set; }

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        public List<string> Members { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the purpose.
        /// </summary>
        public SlackChannelPurpose Purpose { get; set; }

        /// <summary>
        /// Gets or sets the topic.
        /// </summary>
        public SlackChannelTopic Topic { get; set; }
    }
}
