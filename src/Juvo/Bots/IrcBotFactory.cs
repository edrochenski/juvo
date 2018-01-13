﻿// <copyright file="IrcBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Default IRC Bot factory.
    /// </summary>
    public class IrcBotFactory : IIrcBotFactory
    {
        private readonly ILogManager logManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcBotFactory"/> class.
        /// </summary>
        /// <param name="logManager">Log manager.</param>
        public IrcBotFactory(ILogManager logManager)
        {
            this.logManager = logManager;
        }

        /// <inheritdoc />
        public IIrcBot Create(IrcConfigConnection config, IJuvoClient host)
        {
            return new IrcBot(config, host);
        }
    }
}
