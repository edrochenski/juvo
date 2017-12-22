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
        private readonly IClientWebSocket clientWebSocket;
        private readonly DiscordClient discordClient;
        private readonly IJuvoClient juvoClient;
        private bool isDisposed;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBot"/> class.
        /// </summary>
        /// <param name="config">Discord config file.</param>
        /// <param name="host">Host client.</param>
        /// <param name="httpClient">Http client.</param>
        /// <param name="clientWebSocket">Client web socket.</param>
        public DiscordBot(
            DiscordConfigConnection config,
            IJuvoClient host,
            IHttpClient httpClient,
            IClientWebSocket clientWebSocket)
        {
            this.discordClient = new DiscordClient(
                clientWebSocket,
                httpClient,
                new DiscordClientOptions { AuthToken = config.AuthToken, IsBot = true });
            this.juvoClient = host;
            this.clientWebSocket = clientWebSocket;
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
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public Task QueueResponse(IBotCommand cmd)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clean up object and release resources.
        /// </summary>
        /// <param name="isDisposing">Is is currently disposing.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            try
            {
                if (!this.isDisposed && isDisposing)
                {
                    this.clientWebSocket.Dispose();
                }
            }
            finally
            {
                this.isDisposed = true;
            }
        }
    }
}
