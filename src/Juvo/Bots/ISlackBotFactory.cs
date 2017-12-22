// <copyright file="ISlackBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Represents a Slack Bot Factory.
    /// </summary>
    public interface ISlackBotFactory
    {
        /// <summary>
        /// Creates a new slack bot instance.
        /// </summary>
        /// <param name="config">Bot's configuration.</param>
        /// <param name="client">Juvo client hosting the bot.</param>
        /// <returns>Instance of an <see cref="ISlackBot"/>.</returns>
        ISlackBot Create(SlackConfigConnection config, IJuvoClient client);
    }
}
