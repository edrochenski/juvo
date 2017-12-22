// <copyright file="DiscordBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;
    using JuvoProcess.Net.Discord;

    /// <summary>
    /// Discord bot.
    /// </summary>
    public class DiscordBot : IDiscordBot
    {
/*/ Fields /*/
        private readonly DiscordClient discordClient;
        private readonly IJuvoClient juvoClient;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBot"/> class.
        /// </summary>
        /// <param name="config">Discord config file.</param>
        /// <param name="host">Host client.</param>
        public DiscordBot(DiscordConfigConnection config, IJuvoClient host)
        {
            this.discordClient = new DiscordClient(
                new ClientWebSocketProxy(),
                new HttpClientProxy(),
                new DiscordClientOptions { AuthToken = config.AuthToken, IsBot = true });
            this.juvoClient = host;
        }

/*/ Properties /*/

        /// <inheritdoc/>
        public BotType Type => BotType.Discord;

/*/ Methods /*/

        /// <summary>
        /// Starts the bot's connection process.
        /// </summary>
        /// <returns>A Task object associated with the async operation.</returns>
        public async Task Connect()
        {
            await this.discordClient.Connect();
        }

        /// <inheritdoc/>
        public Task QueueResponse(IBotCommand cmd)
        {
            throw new NotImplementedException();
        }
    }
}
