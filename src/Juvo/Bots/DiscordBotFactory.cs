// <copyright file="DiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System.Net.Http;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;
    using JuvoProcess.Net.Discord;

    /// <summary>
    /// Default Discord Bot factory.
    /// </summary>
    public class DiscordBotFactory : IDiscordBotFactory
    {
        private readonly ILogManager logManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBotFactory"/> class.
        /// </summary>
        /// <param name="logManager">Log manager.</param>
        public DiscordBotFactory(ILogManager logManager)
        {
            this.logManager = logManager;
        }

        /// <inheritdoc />
        public IDiscordBot Create(
            DiscordConfigConnection config,
            IDiscordClient discordClient,
            IJuvoClient juvoClient)
        {
            // TODO: this should NOT be using concrete classes
            return new DiscordBot(
                config,
                discordClient,
                juvoClient);
        }
    }
}
