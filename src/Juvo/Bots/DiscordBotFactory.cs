// <copyright file="DiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;

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
        public IDiscordBot Create(
            DiscordConfigConnection config,
            IJuvoClient client,
            IHttpClient httpClient,
            IClientWebSocket clientWebSocket)
        {
            return new DiscordBot(config, client, httpClient, clientWebSocket);
        }
    }
}
