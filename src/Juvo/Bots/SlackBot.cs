// <copyright file="SlackBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Slack;
    using log4net;

    /// <summary>
    /// Slack bot.
    /// </summary>
    public class SlackBot : ISlackBot
    {
        /*/ Fields /*/

        private readonly SlackConfigConnection config;
        private readonly string[] greetings = { "Hey there {0}!", "sup {0}! How's it goin?" };
        private readonly IJuvoClient host;
        private readonly ILog log;
        private readonly Random random;
        private readonly SlackClient slackClient;
        private bool isDisposed;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBot"/> class.
        /// </summary>
        /// <param name="config">Slack configuration.</param>
        /// <param name="host">Host client.</param>
        public SlackBot(SlackConfigConnection config, IJuvoClient host)
        {
            Debug.Assert(config != null, "config == null");

            this.config = config;
            this.host = host;
            this.log = LogManager.GetLogger(typeof(SlackBot));

            this.random = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);
            this.slackClient = new SlackClient(config.Token);
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
                    }
                });

                // var greet = greetings[random.Next(0, greetings.Length - 1)];
                // await slackClient.SendMessage(arg.Channel, string.Format(greet, arg.User));
            }

            await Task.CompletedTask;
        }
    }
}
