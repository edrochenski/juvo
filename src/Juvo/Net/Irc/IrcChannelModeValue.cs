// <copyright file="IrcChannelModeValue.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    /// <summary>
    /// IRC channel mode value.
    /// </summary>
    public class IrcChannelModeValue
    {
        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcChannelModeValue"/> class.
        /// </summary>
        /// <param name="mode">Channel mode.</param>
        /// <param name="value">Associated value.</param>
        public IrcChannelModeValue(IrcChannelMode mode, string value)
        {
            this.Mode = mode;
            this.Value = value;
        }

        /*/ Properties /*/

        /// <summary>
        /// Gets or sets the channel mode.
        /// </summary>
        public IrcChannelMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}
