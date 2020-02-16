// <copyright file="IrcBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Irc;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Default IRC Bot factory.
    /// </summary>
    public class IrcBotFactory : IIrcBotFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IrcBotFactory"/> class.
        /// </summary>
        public IrcBotFactory()
        {
        }

        /// <inheritdoc />
        public IIrcBot Create(IrcConfigConnection config, IServiceProvider services, IJuvoClient host)
        {
            var bot = new IrcBot(
                host,
                services.GetService<IIrcClient>(),
                config,
                services.GetService<ILogManager>());
            return bot;
        }
    }
}
