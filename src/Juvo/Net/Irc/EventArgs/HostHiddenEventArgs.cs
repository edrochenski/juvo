// <copyright file="HostHiddenEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;

    /// <summary>
    /// Represents the data from <see cref="IrcClient.HostHidden"/> event.
    /// </summary>
    public class HostHiddenEventArgs : EventArgs
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="HostHiddenEventArgs"/> class.
        /// </summary>
        /// <param name="host">Host used.</param>
        public HostHiddenEventArgs(string host)
        {
            this.Host = host;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string Host { get; set; }
    }
}
