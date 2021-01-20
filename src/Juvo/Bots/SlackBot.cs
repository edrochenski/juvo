// <copyright file="SlackBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Slack;

    /// <summary>
    /// Slack bot.
    /// </summary>
    public class SlackBot : ISlackBot
    {
        /*/ Fields /*/

        private readonly string[] greetings = { "Hey there {0}!", "sup {0}! How's it goin?" };
        private readonly ILog? log;
        private readonly ILogManager? logManager;
        private readonly Random random;
        private readonly ISlackClient slackClient;

        private SlackConfigConnection config;
        private IJuvoClient host;
        private bool isDisposed;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBot"/> class.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="slackClient">Slack client.</param>
        /// <param name="config">Configuration to use.</param>
        /// <param name="logManager">Log manager.</param>
        public SlackBot(IJuvoClient host, ISlackClient slackClient, SlackConfigConnection config, ILogManager? logManager = null)
        {
            this.host = host;
            this.config = config;
            this.logManager = logManager;
            this.log = this.logManager?.GetLogger(typeof(SlackBot));
            this.random = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);

            this.slackClient = slackClient ?? throw new ArgumentNullException(nameof(slackClient));
            this.slackClient.Initialize(config.Token);
            this.slackClient.MessageReceived += this.SlackClient_MessageReceived;
        }

        /*/ Properties /*/

        /// <inheritdoc/>
        public BotType Type { get => BotType.Slack; }

        /*/ Methods /*/

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        /// <returns>Task associated with the async operation.</returns>
        public async Task Connect()
        {
            await this.slackClient.Connect();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async Task QueueResponse(IBotCommand cmd)
        {
            if (/*cmd is SlackBotCommand sbCmd && */!string.IsNullOrEmpty(cmd.ResponseText))
            {
                // For now we just process them right away
                await this.slackClient.SendMessage(cmd.Source.Identifier, cmd.ResponseText);
            }
        }

        /// <inheritdoc/>
        public async Task Quit(string message)
        {
            this.log?.Info($"Quitting: {message ?? "(null)"}");
            await this.slackClient.Disconnect();
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
                }
            }
            finally
            {
                this.isDisposed = true;
            }
        }

        private async Task SlackClient_MessageReceived(object sender, SlackMessage arg)
        {
            if (arg.Text.StartsWith(this.config.CommandToken))
            {
                this.host.QueueCommand(new SlackBotCommand
                {
                    Bot = this,
                    RequestText = arg.Text.Remove(0, this.config.CommandToken.Length),
                    Source = new CommandSource
                    {
                        Identifier = arg.Channel,
                        SourceType = CommandSourceType.ChannelOrGroup
                    },
                    TriggeredBy = CommandTriggerType.User
                });

                // var greet = greetings[random.Next(0, greetings.Length - 1)];
                // await slackClient.SendMessage(arg.Channel, string.Format(greet, arg.User));
            }

            await Task.CompletedTask;
        }
    }
}
