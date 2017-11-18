// <copyright file="SlackBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using BytedownSoftware.Lib.Net.Slack;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Slack;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Slack bot.
    /// </summary>
    public class SlackBot : IBot
    {
/*/ Fields /*/
        private readonly SlackConfigConnection config;
        private readonly string[] greetings = { "Hey there {0}!", "sup {0}! How's it goin?" };
        private readonly JuvoClient host;
        private readonly ILogger<SlackBot> logger;
        private readonly Random random;
        private readonly SlackClient slackClient;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBot"/> class.
        /// </summary>
        /// <param name="host">Host client.</param>
        /// <param name="config">Slack configuration.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public SlackBot(JuvoClient host, SlackConfigConnection config, ILoggerFactory loggerFactory = null)
        {
            Debug.Assert(config != null, "config == null");

            this.config = config;
            this.host = host;

            this.random = new Random(DateTime.Now.Millisecond * DateTime.Now.Second);
            this.slackClient = new SlackClient(config.Token, loggerFactory);
            this.slackClient.MessageReceived += this.SlackClient_MessageReceived;

            if (loggerFactory != null)
            {
                this.logger = loggerFactory.CreateLogger<SlackBot>();
            }
        }

/*/ Properties /*/

        /// <inheritdoc/>
        public BotType Type { get => BotType.Slack; }

/*/ Methods /*/

    // Public

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        /// <returns>Task associated with the async operation.</returns>
        public async Task Connect()
        {
            await this.slackClient.Connect();
        }

        /// <inheritdoc/>
        public async Task QueueResponse(IBotCommand cmd)
        {
            if (cmd is SlackBotCommand sbCmd)
            {
                // For now we just process them right away
                await this.slackClient.SendMessage(sbCmd.Channel, sbCmd.ResponseText);
            }
        }

    // Private
        private async Task SlackClient_MessageReceived(object sender, SlackMessage arg)
        {
            if (arg.Text.StartsWith(this.config.CommandToken))
            {
                this.host.QueueCommand(new SlackBotCommand
                {
                    Bot = this,
                    Channel = arg.Channel,
                    RequestText = arg.Text.Remove(0, this.config.CommandToken.Length)
                });

                // var greet = greetings[random.Next(0, greetings.Length - 1)];
                // await slackClient.SendMessage(arg.Channel, string.Format(greet, arg.User));
            }

            await Task.CompletedTask;
        }
    }
}
