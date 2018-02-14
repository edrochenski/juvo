// <copyright file="MessageReceivedArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;

    /// <summary>
    /// Represents the data from <see cref="IrcClient.MessageReceived" /> event.
    /// </summary>
    public class MessageReceivedArgs : EventArgs
    {
        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceivedArgs"/> class.
        /// </summary>
        /// <param name="message">Message received.</param>
        public MessageReceivedArgs(string message) => this.Message = message;

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the message received.
        /// </summary>
        public string Message { get; set; }
    }
}
