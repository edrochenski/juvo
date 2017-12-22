// <copyright file="IDiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;

    /// <summary>
    /// Represents a Discord Bot Factory.
    /// </summary>
    public interface IDiscordBotFactory
    {
        /// <summary>
        /// Creates a new discord bot instance.
        /// </summary>
        /// <param name="config">Bot's configuration.</param>
        /// <param name="client">Juvo client hosting the bot.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="clientWebSocket">Client web socket.</param>
        /// <returns>Instance of an <see cref="IDiscordBot"/>.</returns>
        IDiscordBot Create(
            DiscordConfigConnection config,
            IJuvoClient client,
            IHttpClient httpClient,
            IClientWebSocket clientWebSocket);
    }
}
