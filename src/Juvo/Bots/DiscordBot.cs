// <copyright file="DiscordBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Discord;
    using JuvoProcess.Net.Discord.Model;
    using static JuvoProcess.Net.Discord.Model.ReadyEventData;

    /// <summary>
    /// Discord bot.
    /// </summary>
    public class DiscordBot : IDiscordBot
    {
        /*/ Fields /*/

        private readonly IDiscordClient discordClient;
        private readonly ILog log;
        private readonly ILogManager logManager;
        private DiscordConfigConnection config;
        private IJuvoClient host;
        private bool isDisposed;
        private ReadyData discordData;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBot"/> class.
        /// </summary>
        /// <param name="discordClient">Discord client.</param>
        /// <param name="logManager">Log manager.</param>
        public DiscordBot(IDiscordClient discordClient, ILogManager logManager)
        {
            this.discordClient = discordClient ?? throw new ArgumentNullException(nameof(discordClient));
            this.logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            this.log = logManager?.GetLogger(typeof(DiscordBot));

            this.discordClient.Disconnected += this.DiscordClient_Disconnected;
            this.discordClient.ReadyReceived += this.DiscordClient_ReadyReceived;
        }

        /*/ Properties /*/

        /// <inheritdoc/>
        public DiscordConfigConnection Configuration => this.config;

        /// <inheritdoc/>
        public BotType Type => BotType.Discord;

        /*/ Methods /*/

        /// <summary>
        /// Starts the bot's connection process.
        /// </summary>
        /// <returns>A Task object associated with the async operation.</returns>
        public async Task Connect()
        {
            if (!this.config.Enabled)
            {
                this.log.Warn("Connect() called on a disabled bot!");
                return;
            }

            await this.discordClient.Connect();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Initialize(DiscordConfigConnection config, IJuvoClient juvoClient)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.host = juvoClient;

            this.discordClient.Initialize(new DiscordClientOptions
            {
                AuthToken = config.AuthToken,
                IsBot = true
            });
        }

        /// <inheritdoc/>
        public Task QueueResponse(IBotCommand cmd)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task Quit(string message)
        {
            await Task.CompletedTask;
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
                    this.discordClient.Dispose();
                }
            }
            finally
            {
                this.isDisposed = true;
            }
        }

        private void DiscordClient_Disconnected(object sender, DisconnectedEventArgs arg)
        {
            if (!arg.UserInitiated)
            {
                this.log.Warn("Disconnected, trying to reconnect...");
                this.discordClient.Connect();
            }
        }

        private void DiscordClient_ReadyReceived(object sender, ReadyEventData data)
        {
            this.discordData = data.Data;
        }
    }
}
