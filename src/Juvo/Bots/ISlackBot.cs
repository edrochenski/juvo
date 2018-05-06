// <copyright file="ISlackBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Represents a Slack Bot.
    /// </summary>
    public interface ISlackBot : IBot
    {
        /// <summary>
        /// Intializes the bot.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        /// <param name="host">Host.</param>
        void Initialize(SlackConfigConnection config, IJuvoClient host);
    }
}
