// <copyright file="SlackBotCommand.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Bots
{
    /// <summary>
    /// Command for a SlackBot.
    /// </summary>
    public struct SlackBotCommand : IBotCommand
    {
        /// <summary>
        /// Gets or sets the bot the command was sent to/from.
        /// </summary>
        public IBot Bot { get; set; }

        /// <summary>
        /// Gets or sets the channel for the command.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets the text of the request.
        /// </summary>
        public string RequestText { get; set; }

        /// <summary>
        /// Gets or sets the text of the response.
        /// </summary>
        public string ResponseText { get; set; }
    }
}
