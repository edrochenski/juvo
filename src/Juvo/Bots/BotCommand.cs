// <copyright file="BotCommand.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    /// <summary>
    /// Generic bot command.
    /// </summary>
    public class BotCommand : IBotCommand
    {
        /// <inheritdoc/>
        public IBot Bot { get; set; }

        /// <inheritdoc/>
        public string RequestText { get; set; }

        /// <inheritdoc/>
        public string ResponseText { get; set; }

        /// <inheritdoc/>
        public CommandSource Source { get; set; }
    }
}
