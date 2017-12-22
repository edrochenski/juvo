﻿// <copyright file="SlackBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Default Slack Bot factory.
    /// </summary>
    public class SlackBotFactory : ISlackBotFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBotFactory"/> class.
        /// </summary>
        public SlackBotFactory()
        {
        }

        /// <inheritdoc />
        public ISlackBot Create(SlackConfigConnection config, IJuvoClient host)
        {
            return new SlackBot(config, host);
        }
    }
}