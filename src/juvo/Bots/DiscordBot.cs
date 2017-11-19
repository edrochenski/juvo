// <copyright file="DiscordBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;
    using JuvoProcess.Net.Discord;

    /// <summary>
    /// Discord bot.
    /// </summary>
    public class DiscordBot : IBot
    {
/*/ Fields /*/
        private readonly DiscordClient discordClient;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBot"/> class.
        /// </summary>
        /// <param name="host">Host client.</param>
        /// <param name="config">Discord config file.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DiscordBot(
            JuvoClient host,
            DiscordConfigConnection config)
        {
            this.discordClient = new DiscordClient(
                new ClientWebSocketProxy(),
                new HttpClientProxy(),
                new DiscordClientOptions { AuthToken = config.AuthToken, IsBot = true });
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
