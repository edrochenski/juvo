// <copyright file="ChannelModeChangedEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the data from a <see cref="IrcClient.ChannelModeChanged"/> event.
    /// </summary>
    public class ChannelModeChangedEventArgs : EventArgs
    {
        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelModeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="channel">Channel where mode changed.</param>
        /// <param name="added">Modes added.</param>
        /// <param name="removed">Modes removed.</param>
        public ChannelModeChangedEventArgs(
            string channel,
            IEnumerable<IrcChannelModeValue> added,
            IEnumerable<IrcChannelModeValue> removed)
        {
            this.Added = added;
            this.Removed = removed;
            this.Channel = channel;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets added modes.
        /// </summary>
        public IEnumerable<IrcChannelModeValue> Added { get; set; }

        /// <summary>
        /// Gets or sets removed modes.
        /// </summary>
        public IEnumerable<IrcChannelModeValue> Removed { get; set; }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public string Channel { get; set; }
    }
}
