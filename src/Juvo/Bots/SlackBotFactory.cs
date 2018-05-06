// <copyright file="SlackBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Slack;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Default Slack Bot factory.
    /// </summary>
    public class SlackBotFactory : ISlackBotFactory
    {
        private readonly ILogManager logManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBotFactory"/> class.
        /// </summary>
        /// <param name="logManager">Log mananger.</param>
        public SlackBotFactory(ILogManager logManager)
        {
            this.logManager = logManager;
        }

        /// <inheritdoc />
        public ISlackBot Create(SlackConfigConnection config, IJuvoClient host)
        {
            var bot = new SlackBot(
                Program.Instance.Services.GetService<ISlackClient>(),
                Program.Instance.Services.GetService<ILogManager>());
            bot.Initialize(config, host);

            return bot;
        }
    }
}
