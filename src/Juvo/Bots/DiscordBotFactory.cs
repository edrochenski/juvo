// <copyright file="DiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Discord;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Default Discord Bot factory.
    /// </summary>
    public class DiscordBotFactory : IDiscordBotFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBotFactory"/> class.
        /// </summary>
        public DiscordBotFactory()
        {
        }

        /// <inheritdoc />
        public IDiscordBot Create(DiscordConfigConnection config, IServiceProvider services, IJuvoClient juvoClient)
        {
            var bot = new DiscordBot(
                services.GetService<IDiscordClient>(),
                services.GetService<ILogManager>());
            bot.Initialize(config, juvoClient);

            return bot;
        }
    }
}
