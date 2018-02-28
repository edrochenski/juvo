// <copyright file="IDiscordBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Represents a discord bot.
    /// </summary>
    public interface IDiscordBot : IBot
    {
        /// <summary>
        /// Gets the bot's configuration.
        /// </summary>
        DiscordConfigConnection Configuration { get; }
    }
}
