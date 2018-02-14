// <copyright file="SocketEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Represents the data associated with a Socket event.
    /// </summary>
    public class SocketEventArgs : EventArgs
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketEventArgs"/> class.
        /// </summary>
        /// <param name="error">Error object.</param>
        /// <param name="errorText">Text of the error.</param>
        public SocketEventArgs(SocketError error, string errorText)
        {
            this.Error = error;
            this.ErrorText = errorText;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the Error.
        /// </summary>
        public SocketError Error { get; set; }

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText { get; set; }
    }
}
