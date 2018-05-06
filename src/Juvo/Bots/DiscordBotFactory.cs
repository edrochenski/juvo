// <copyright file="DiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Discord;
    using Microsoft.Extensions.DependencyInjection;

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
        public IDiscordBot Create(DiscordConfigConnection config, IJuvoClient juvoClient)
        {
            var bot = new DiscordBot(
                Program.Instance.Services.GetService<IDiscordClient>(),
                Program.Instance.Services.GetService<ILogManager>());
            bot.Initialize(config, juvoClient);

            return bot;
        }
    }
}
