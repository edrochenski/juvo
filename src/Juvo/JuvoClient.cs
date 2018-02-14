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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.Modules;
    using JuvoProcess.Modules.HackerNews;
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
        private readonly Dictionary<string[], Func<IBotCommand, Task>> commands;
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
        private string lastPerf;
        private DateTime lastPerfTime;
        private Mutex lastPerfLock;
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
            this.log = logManager?.GetLogger(typeof(JuvoClient));
            this.started = DateTime.UtcNow;
            this.State = JuvoState.Idle;
            this.sysInfo = GetSystemInfo();
            this.lastPerfLock = new Mutex();

            // this block maps the bots internal commands to the appropriate
            // internal methods.
            // ?: should these commands be overridable in some way, for example
            // if a plugin/module wanted to handle one of these.
            this.commands = new Dictionary<string[], Func<IBotCommand, Task>>
            {
                { new[] { "shutdown", "die" }, this.CommandShutdown },
                { new[] { "perf" }, this.CommandPerf },
                { new[] { "status" }, this.CommandStatus }
            };

            // this block maps commands to plugins/modules.
            // TODO: this needs to be externalized
            this.modules = new Dictionary<string[], IBotModule>
            {
                // TODO: modules will need to ask for dependencies differently
                {
                    new[] { "gps", "sky", "weather" },
                    new WeatherModule(this, new HttpClientProxy(new HttpClientHandlerProxy()))
                },
                {
                    new[] { "hn" },
                    new HackerNewsModule(this, new HttpClientProxy(new HttpClientHandlerProxy()))
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
                        this.ProcessCommand(cmd).Wait();
                    }
                }
            }

            lock (this.lastPerfLock)
            {
                if (DateTime.Now.Minute % 5 == 0 && DateTime.Now.Second == 0 &&
                    (this.lastPerf == null || this.lastPerfTime.AddMinutes(5) < DateTime.UtcNow))
                {
                    this.LogPerf();
                }
            }
        }

        /// <summary>
        /// Commands the bot to return the latest perf results.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandPerf(IBotCommand command)
        {
            command.ResponseText = this.lastPerf ?? "No performance info collected yet.";
            await Task.CompletedTask;
        }

        /// <summary>
        /// Commands the bot to shutdown.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandShutdown(IBotCommand command)
        {
            this.log?.Info($"Shutting down...");
            await this.StopBots("Requested");
            Environment.Exit(0);
        }

        /// <summary>
        /// Commands the bot to return the status of the bot.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandStatus(IBotCommand command)
        {
            var status = new StringBuilder();

            if (this.ircBots.Any())
            {
                status.Append("[IRC] ");
                foreach (var bot in this.ircBots)
                {
                    status.Append($"{bot.GetHashCode()} ");
                }
            }

            if (this.discordBots.Any())
            {
                status.Append("[Discord] ");
                foreach (var bot in this.discordBots)
                {
                    status.Append($"{bot.GetHashCode()} ");
                }
            }

            if (this.slackBots.Any())
            {
                status.Append("[Slack] ");
                foreach (var bot in this.slackBots)
                {
                    status.Append($"{bot.GetHashCode()} ");
                }
            }

            command.ResponseText = status.ToString();
            await Task.CompletedTask;
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
                result.LocalAppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%HOME%/.juvo"));
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
            this.log?.Info("Loading configuration");

            var file = Path.Combine(this.sysInfo.AppDataPath.FullName, ConfigFileName);
            if (!File.Exists(file))
            {
                this.log?.Error($"Configuration file is missing ({file})");
                Environment.Exit(-1);
            }

            var json = File.ReadAllText(file);
            this.config = JsonConvert.DeserializeObject<Config>(json);

            if (this.config == null)
            {
                this.log?.Error($"Configuration file could not be loaded (length: {json.Length})");
                Environment.Exit(-1);
            }

            foreach (var disc in this.config?.Discord?.Connections?.Where(x => x.Enabled))
            {
                var dc = new DiscordClient(
                    new ClientWebSocketProxy(),
                    new HttpClientProxy(new HttpClientHandlerProxy()),
                    new DiscordClientOptions { AuthToken = disc.AuthToken, IsBot = true });
                this.discordBots.Add(this.discordBotFactory.Create(disc, dc, this));
            }

            foreach (var irc in this.config?.Irc?.Connections?.Where(x => x.Enabled))
            {
                this.ircBots.Add(this.ircBotFactory.Create(irc, this));
            }

            foreach (var slack in this.config?.Slack?.Connections?.Where(x => x.Enabled))
            {
                this.slackBots.Add(this.slackBotFactory.Create(slack, this));
            }
        }

        private void LogPerf()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                this.lastPerf =
                    $"[PRC] " +
                    $"Threads:{proc.Threads.Count} " +
                    $"Handles:{proc.HandleCount} " +
                    $"TPT:{proc.TotalProcessorTime.TotalSeconds:0.00}s " +
                    $"PPT:{proc.PrivilegedProcessorTime.TotalSeconds:0.00}s " +
                    $"UPT:{proc.TotalProcessorTime.TotalSeconds:0.00}s " +
                    $"[MEM] " +
                    $"WS:{this.ToMb(proc.WorkingSet64):0.00}mB " +
                    $"PM:{this.ToMb(proc.PrivateMemorySize64):0.00}mB " +
                    $"PS:{this.ToMb(proc.PagedSystemMemorySize64):0.00}mB " +
                    $"NP:{this.ToMb(proc.NonpagedSystemMemorySize64):0.00}mB " +
                    $"[INF] " +
                    $"PID:{proc.Id} " +
                    $"SID:{proc.SessionId} " +
                    $"Up:{DateTime.Now - proc.StartTime} " +
                    $"Taken:{DateTime.UtcNow} UTC";
                this.lastPerfTime = DateTime.UtcNow;
            }

            this.log?.Info(this.lastPerf);
        }

        private async Task ProcessCommand(IBotCommand cmd)
        {
            this.log?.Info("Processing command");
            var cmdName = cmd.RequestText.Split(' ')[0].ToLowerInvariant();

            var command = this.commands.SingleOrDefault(c => c.Key.Contains(cmdName));
            if (command.Value != null)
            {
                try
                {
                    await command.Value(cmd);
                }
                catch (Exception exc)
                {
                    var message = $"Error executing command '{command.Value.Method.Name}'";
                    this.log?.Error(message, exc);
                    cmd.ResponseText = message;
                }
            }
            else
            {
                var module = this.modules.SingleOrDefault(p => p.Key.Contains(cmdName));
                if (module.Value != null)
                {
                    try
                    {
                        await module.Value.Execute(cmd);
                    }
                    catch (Exception exc)
                    {
                        var message = $"Error executing module '{module.Value.GetType().Name}'";
                        this.log?.Error(message, exc);
                        cmd.ResponseText = message;
                    }
                }
                else
                {
                    cmd.ResponseText = "Invalid command";
                }
            }

            this.log?.Info("Queueing response on bot");
            await cmd.Bot.QueueResponse(cmd);
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

        private async Task StopBots(string quitMessage)
        {
            foreach (var bot in this.discordBots)
            {
                await bot.Quit(quitMessage);
            }

            foreach (var bot in this.ircBots)
            {
                await bot.Quit(quitMessage);
            }

            foreach (var bot in this.slackBots)
            {
                await bot.Quit(quitMessage);
            }
        }

        private double ToMb(long bytes)
        {
            return bytes / (1024D * 1024D);
        }
    }
}
