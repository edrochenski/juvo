// <copyright file="IIrcBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Irc;

    /// <summary>
    /// Represents an IRC Bot.
    /// </summary>
    public interface IIrcBot : IBot
    {
        /// <summary>
        /// Gets the bot's current nickname.
        /// </summary>
        string CurrentNickname { get; }

        /// <summary>
        /// Gets the bot's network.
        /// </summary>
        IrcNetwork Network { get; }

        /// <summary>
        /// Initializes the bot.
        /// </summary>
        /// <param name="config">IRC configuration.</param>
        /// <param name="juvoClient">Host.</param>
        void Initialize(IrcConfigConnection config, IJuvoClient juvoClient);
    }
}
