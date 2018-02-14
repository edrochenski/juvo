// <copyright file="ChannelUserEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;

    /// <summary>
    /// Represents the data from ChannelUser event.
    /// </summary>
    public class ChannelUserEventArgs : EventArgs
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelUserEventArgs"/> class.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="user">User.</param>
        /// <param name="isOwned">Is owned.</param>
        public ChannelUserEventArgs(
            string channel,
            IrcUser user,
            bool isOwned)
            : this(channel, user, isOwned, null, IrcMessageType.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelUserEventArgs"/> class.
        /// </summary>
        /// <param name="channel">Channel.</param>
        /// <param name="user">User.</param>
        /// <param name="isOwned">Is owned.</param>
        /// <param name="message">Message.</param>
        /// <param name="messageType">Message type.</param>
        public ChannelUserEventArgs(
            string channel,
            IrcUser user,
            bool isOwned,
            string message,
            IrcMessageType messageType)
        {
            this.Channel = channel;
            this.IsOwned = isOwned;
            this.Message = message;
            this.MessageType = messageType;
            this.User = user;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the even is owned.
        /// </summary>
        public bool IsOwned { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public IrcMessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public IrcUser User { get; set; }
    }
}
