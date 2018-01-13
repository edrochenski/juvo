// <copyright file="JuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.Modules;
    using JuvoProcess.Modules.Weather;
    using JuvoProcess.Net;
    using JuvoProcess.Net.Discord;
    using Newtonsoft.Json;

    /// <summary>
    /// Juvo client.
    /// </summary>
    public class JuvoClient : IJuvoClient
    {
/*/ Constants /*/
        private const string ConfigFileName = "config.json";
        private const int TimerTickRate = 10;

/*/ Fields /*/
        private readonly Queue<IBotCommand> commandQueue;
        private readonly Timer commandTimer;
        private readonly IDiscordBotFactory discordBotFactory;
        private readonly List<IDiscordBot> discordBots;
        private readonly IIrcBotFactory ircBotFactory;
        private readonly List<IIrcBot> ircBots;
        private readonly ILog log;
        private readonly Dictionary<string[], IBotModule> modules;
        private readonly ISlackBotFactory slackBotFactory;
        private readonly List<ISlackBot> slackBots;
        private readonly ManualResetEvent resetEvent;
        private readonly SystemInfo sysInfo;

        private Config config;
        private DateTime started;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="JuvoClient"/> class.
        /// </summary>
        /// <param name="discordBotFactory">Factory object for Discord bots.</param>
        /// <param name="ircBotFactory">Factory object for IRC bots.</param>
        /// <param name="slackBotFactory">Factory object for Slack bots.</param>
        /// <param name="logManager">Log manager.</param>
        /// <param name="resetEvent">Manual reset object for thread.</param>
        public JuvoClient(
            IDiscordBotFactory discordBotFactory,
            IIrcBotFactory ircBotFactory,
            ISlackBotFactory slackBotFactory,
            ILogManager logManager,
            ManualResetEvent resetEvent = null)
        {
            this.discordBotFactory = discordBotFactory ?? throw new ArgumentNullException(nameof(discordBotFactory));
            this.ircBotFactory = ircBotFactory ?? throw new ArgumentNullException(nameof(ircBotFactory));
            this.resetEvent = resetEvent;
            this.slackBotFactory = slackBotFactory ?? throw new ArgumentNullException(nameof(slackBotFactory));

            this.commandQueue = new Queue<IBotCommand>();
            this.commandTimer = new Timer(this.CommandTimerTick, null, TimerTickRate, TimerTickRate);
            this.discordBots = new List<IDiscordBot>();
            this.ircBots = new List<IIrcBot>();
            this.slackBots = new List<ISlackBot>();
            this.log = (ILog)logManager?.GetLogger(typeof(JuvoClient));
            this.started = DateTime.UtcNow;
            this.State = JuvoState.Idle;
            this.sysInfo = GetSystemInfo();

            this.modules = new Dictionary<string[], IBotModule>
            {
                // TODO: modules will need to ask for dependencies differently
                {
                    new[] { "gps", "sky", "weather" },
                    new WeatherModule(this, new HttpClientProxy(new HttpClientHandlerProxy()))
                }
            };
        }

/*/ Properties /*/

        /// <inheritdoc/>
        public Config Config => this.config;

        /// <inheritdoc/>
        public ILog Log => this.log;

        /// <summary>
        /// Gets or sets the bot's current state.
        /// </summary>
        public JuvoState State { get; protected set; }

        /// <inheritdoc/>
        public SystemInfo SystemInfo => this.sysInfo;

/*/ Methods /*/

    // Public

        /// <summary>
        /// Queues command for the bot to execute or pass on.
        /// </summary>
        /// <param name="cmd">Command object.</param>
        public void QueueCommand(IBotCommand cmd)
        {
            Debug.Assert(cmd != null, "cmd == null");
            Debug.Assert(!string.IsNullOrEmpty(cmd.RequestText), "cmd.RequestText != null/empty");

            this.commandQueue.Enqueue(cmd);
            this.log?.Info("Queued command");
        }

        /// <summary>
        /// Starts the bot.
        /// </summary>
        /// <returns>Result of the call.</returns>
        public async Task<int> Run()
        {
            this.log?.Info("Gathering system information");
            GetSystemInfo();

            this.log?.Info("Creating any missing resources");
            this.CreateResources();

            this.log?.Info("Loading configuration file");
            this.LoadConfig();

            this.log?.Info("Starting bots");
            await this.StartBots();

            this.log?.Info("Juvo is now running");
            this.State = JuvoState.Running;

            return await Task.FromResult(0);
        }

    // Protected

        /// <summary>
        /// Called when <see cref="commandTimer" /> tick occurs.
        /// </summary>
        /// <param name="state">State object.</param>
        protected void CommandTimerTick(object state)
        {
            if (this.commandQueue.Count > 0)
            {
                lock (this)
                {
                    IBotCommand cmd;
                    while (this.commandQueue.Count > 0 && (cmd = this.commandQueue.Dequeue()) != null)
                    {
                        this.ProcessCommand(cmd);
                    }
                }
            }
        }

    // Private
    // |
        private static string GetDefaultConfig()
        {
            // TODO: decide if we should use a resource instead
            var nl = Environment.NewLine;
            return "{" + nl +
                   "  \"irc\": {" + nl +
                   "    \"nickname\": \"juvo\"," + nl +
                   "    \"realName\": \"juvo\"," + nl +
                   "    \"username\": \"juvo\"," + nl +
                   "    \"connections\": [" + nl +
                   "      {" + nl +
                   "        \"name\": \"undernet\"," + nl +
                   "        \"servers\": [" + nl +
                   "          {" + nl +
                   "            \"host\": \"elysium.us.ix.undernet.org\"," + nl +
                   "            \"port\": 6667" + nl +
                   "          }," + nl +
                   "          {" + nl +
                   "            \"host\": \"eu.undernet.org\"," + nl +
                   "            \"port\": 6667" + nl +
                   "          }" + nl +
                   "        ]," + nl +
                   "        \"channels\": [" + nl +
                   "          {" + nl +
                   "            \"name\": \"#bytedown\"" + nl +
                   "          }," + nl +
                   "        ]" + nl +
                   "      }" + nl +
                   "    ]" + nl +
                   "  }" + nl +
                   "}";
        }

        private static SystemInfo GetSystemInfo()
        {
            var result = new SystemInfo { AppDataPath = null, Os = OperatingSystem.Unknown };

            // TODO: extract constant path/dir name
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result.AppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%APPDATA%/juvo"));
                result.LocalAppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/juvo"));
                result.Os = OperatingSystem.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                result.AppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%HOME%/.juvo"));
                result.LocalAppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%HOME%/juvo"));
                result.Os = OperatingSystem.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // TODO: Set AppDataPath on OSX
                result.Os = OperatingSystem.Osx;
            }

            return result;
        }

        private void CreateAppFolders()
        {
            Directory.CreateDirectory(this.sysInfo.AppDataPath.FullName);
            Directory.CreateDirectory(this.sysInfo.LocalAppDataPath.FullName);
        }

        private void CreateConfigFile()
        {
            var appData = this.sysInfo.AppDataPath.FullName;
            if (!File.Exists(Path.Combine(appData, ConfigFileName)))
            {
                using (var file = File.CreateText(Path.Combine(appData, ConfigFileName)))
                {
                    file.Write(GetDefaultConfig());
                    file.Flush();
                }
            }
        }

        private void CreateResources()
        {
            this.CreateAppFolders();
            this.CreateConfigFile();
        }

        private void LoadConfig()
        {
            var file = Path.Combine(this.sysInfo.AppDataPath.FullName, ConfigFileName);
            if (File.Exists(file))
            {
                this.log.Info("Loading configuration");
                var json = File.ReadAllText(file);
                this.config = JsonConvert.DeserializeObject<Config>(json);
            }

            foreach (var disc in this.config?.Discord?.Connections.Where(x => x.Enabled))
            {
                var dc = new DiscordClient(
                    new ClientWebSocketProxy(),
                    new HttpClientProxy(new HttpClientHandlerProxy()),
                    new DiscordClientOptions { AuthToken = disc.AuthToken, IsBot = true });
                this.discordBots.Add(this.discordBotFactory.Create(disc, dc, this));
            }

            foreach (var irc in this.config?.Irc?.Connections.Where(x => x.Enabled))
            {
                this.ircBots.Add(this.ircBotFactory.Create(irc, this));
            }

            foreach (var slack in this.config?.Slack?.Connections.Where(x => x.Enabled))
            {
                this.slackBots.Add(this.slackBotFactory.Create(slack, this));
            }
        }

        private void ProcessCommand(IBotCommand cmd)
        {
            this.log?.Info("Processing command");
            var cmdName = cmd.RequestText.Split(' ')[0].ToLowerInvariant();
            var module = this.modules.SingleOrDefault(p => p.Key.Contains(cmdName));
            if (module.Value != null)
            {
                try
                {
                    module.Value.Execute(cmd);
                }
                catch (Exception exc)
                {
                    var message = $"Error executing module '{module.GetType().Name}'";
                    this.log?.Error(message, exc);
                    cmd.ResponseText = message;
                }
            }
            else
            {
                cmd.ResponseText = "Invalid command";
            }

            this.log?.Info("Queueing response on bot");
            cmd.Bot.QueueResponse(cmd);
        }

        private async Task StartBots()
        {
            await this.StartDiscordBots();
            await this.StartIrcBots();
            await this.StartSlackBots();
        }

        private async Task StartDiscordBots()
        {
            foreach (var bot in this.discordBots)
            {
                await bot.Connect();
            }
        }

        private async Task StartIrcBots()
        {
            foreach (var bot in this.ircBots)
            {
                await bot.Connect();
            }
        }

        private async Task StartSlackBots()
        {
            foreach (var bot in this.slackBots)
            {
                await bot.Connect();
            }
        }
    }
}
