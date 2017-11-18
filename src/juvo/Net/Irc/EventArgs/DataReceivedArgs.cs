// <copyright file="DataReceivedArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;

    /// <summary>
    /// Represents the data from DataReceived event.
    /// </summary>
    public class DataReceivedArgs : EventArgs
    {
        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReceivedArgs"/> class.
        /// </summary>
        /// <param name="data">Data received.</param>
        public DataReceivedArgs(byte[] data) => this.Data = data;

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public byte[] Data { get; set; }
    }
}
