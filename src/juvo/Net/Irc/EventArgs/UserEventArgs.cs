// <copyright file="UserEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;

    /// <summary>
    /// Represents the data from User event.
    /// </summary>
    public class UserEventArgs : EventArgs
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEventArgs"/> class.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="message">Message.</param>
        /// <param name="isOwned">Is owned.</param>
        public UserEventArgs(IrcUser user, string message, bool isOwned)
            : this(user, message, isOwned, IrcMessageType.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEventArgs"/> class.
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="message">Message.</param>
        /// <param name="isOwned">Is owned.</param>
        /// <param name="messageType">Message type.</param>
        public UserEventArgs(IrcUser user, string message, bool isOwned, IrcMessageType messageType)
        {
            this.IsOwned = isOwned;
            this.Message = message;
            this.MessageType = messageType;
            this.User = user;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets a value indicating whether is owned.
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
