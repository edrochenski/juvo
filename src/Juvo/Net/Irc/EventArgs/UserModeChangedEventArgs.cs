// <copyright file="UserModeChangedEventArgs.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the data from a <see cref="IrcClient.UserModeChanged"/> event.
    /// </summary>
    public class UserModeChangedEventArgs : EventArgs
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="UserModeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="added">Added modes.</param>
        /// <param name="removed">Removed modes.</param>
        public UserModeChangedEventArgs(
            IEnumerable<IrcUserMode> added,
            IEnumerable<IrcUserMode> removed)
        {
            this.Added = added;
            this.Removed = removed;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the added modes.
        /// </summary>
        public IEnumerable<IrcUserMode> Added { get; protected set; }

        /// <summary>
        /// Gets or sets the removed modes.
        /// </summary>
        public IEnumerable<IrcUserMode> Removed { get; protected set; }
    }
}
