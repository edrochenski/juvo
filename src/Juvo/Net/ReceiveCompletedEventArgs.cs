// <copyright file="ReceiveCompletedEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;

    /// <summary>
    /// Represents the data from a <see cref="SocketClient.ReceiveCompleted"/> event.
    /// </summary>
    public class ReceiveCompletedEventArgs : EventArgs
    {
        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiveCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="length">Length of the data received.</param>
        /// <param name="data">Data received.</param>
        public ReceiveCompletedEventArgs(int length, byte[] data)
        {
            this.Data = data;
            this.Length = length;
        }

        /*/ Properties /*/

        /// <summary>
        /// Gets or sets the data received.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the length of the data received.
        /// </summary>
        public int Length { get; set; }
    }
}
