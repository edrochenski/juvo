// <copyright file="IrcBotCommand.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    /// <summary>
    /// Represents an IRC bot command.
    /// </summary>
    public struct IrcBotCommand : IBotCommand
    {
        /// <inheritdoc/>
        public IBot Bot { get; set; }

        /// <summary>
        /// Gets or sets the channel the command was issued in.
        /// </summary>
        public string Channel { get; set; }

        /// <inheritdoc/>
        public string RequestText { get; set; }

        /// <inheritdoc/>
        public string ResponseText { get; set; }
    }
}
