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
        /// <summary>
        /// Initializes a new instance of the <see cref="BotCommand"/> class.
        /// </summary>
        /// <param name="bot">Bot that received the command.</param>
        /// <param name="triggeredBy">Source that triggered the command.</param>
        /// <param name="source">Source of the command.</param>
        /// <param name="request">Request text.</param>
        public BotCommand(IBot? bot, CommandTriggerType triggeredBy, CommandSource source, string request)
        {
            this.Bot = bot;
            this.Source = source;
            this.RequestText = request;
            this.TriggeredBy = triggeredBy;
        }

        /// <inheritdoc/>
        public IBot? Bot { get; set; }

        /// <inheritdoc/>
        public string RequestText { get; set; }

        /// <inheritdoc/>
        public string? ResponseText { get; set; }

        /// <inheritdoc/>
        public CommandSource Source { get; set; }

        /// <inheritdoc/>
        public CommandTriggerType TriggeredBy { get; set; }
    }
}
