﻿// <copyright file="JuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using AngleSharp.Parser.Html;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Discord;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Juvo client.
    /// </summary>
    public class JuvoClient
    {
/*/ Constants /*/
        private const string ConfigFileName = "config.json";
        private const int TimerTickRate = 100;

/*/ Fields /*/
        private readonly Queue<IBotCommand> commandQueue;
        private readonly Timer commandTimer;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<JuvoClient> logger;
        private readonly ManualResetEvent resetEvent;
        private readonly SystemInfo sysInfo;

        private Config config;
        private List<DiscordBot> discordBots;
        private List<IrcBot> ircBots;
        private List<SlackBot> slackBots;
        private DateTime started;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="JuvoClient"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory object to create logger.</param>
        public JuvoClient(ILoggerFactory loggerFactory)
            : this(loggerFactory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JuvoClient"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory object to create logger.</param>
        /// <param name="resetEvent">Manual reset object for thread.</param>
        public JuvoClient(ILoggerFactory loggerFactory, ManualResetEvent resetEvent)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.resetEvent = resetEvent;

            this.commandQueue = new Queue<IBotCommand>();
            this.commandTimer = new Timer(this.CommandTimerTick, null, TimerTickRate, TimerTickRate);
            this.logger = loggerFactory.CreateLogger<JuvoClient>();
            this.started = DateTime.UtcNow;
            this.State = JuvoState.Idle;
            this.sysInfo = GetSystemInfo();
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the bot's current state.
        /// </summary>
        public JuvoState State { get; protected set; }

/*/ Methods /*/

    // Public

        /// <summary>
        /// Processes the weather command.
        /// </summary>
        /// <param name="cmd">Command object.</param>
        public void CommandWeather(IBotCommand cmd)
        {
            // try
            // {
            //     var cmdParts = cmd.RequestText.Split(' ');
            //     var location = string.Join("+", cmdParts, 1, cmdParts.Length - 1);
            //     using (var handler = new HttpClientHandler())
            //     using (var client = new HttpClient(handler))
            //     {
            //         var parser = new HtmlParser();
            //         var gpsUrl = $"https://www.bing.com/search?q={location}+longitude+latitude&qs=n&form=QBLH";
            //         using (var gpsResponse = client.GetAsync(gpsUrl).Result)
            //         using (var gpsDoc = parser.Parse(gpsResponse.Content.ReadAsStringAsync().Result))
            //         {
            //             var input = gpsDoc.GetElementById("mt_toTextBox_dvc_2");
            //             if (input?.GetAttribute("value") != "")
            //             {
            //                 var latLon = input.GetAttribute("value");
            //                 var dsUrl = $"https://api.darksky.net/forecast/a4b34f1591b0da62f90e1eb28d1eb627/{latLon}";
            //                 using (var dsResponse = client.GetAsync(dsUrl).Result)
            //                 {
            //                     var json = dsResponse.Content.ReadAsStringAsync().Result;
            //                     File.WriteAllText("darksky.json", json);
            //                     var ds = JsonConvert.DeserializeObject<DarkSkyResponse>(json);
            //                     if (ds != null)
            //                     {
            //                         var updated = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(ds.Currently.Time);
            //                         cmd.ResponseText = $"Currently: {ds.Currently.Summary} and " +
            //                                            $"{ds.Currently.Temperature:.0}°F (Feels like {ds.Currently.ApparentTemperature:.0}°F) " +
            //                                            $"| Dew Point: {ds.Currently.DewPoint:.00}°F, Humidity: {(ds.Currently.Humidity*100):.0}%, " +
            //                                            $"Pressure: {ds.Currently.Pressure:.00}mb, UV Index: {ds.Currently.UvIndex:0} " +
            //                                            $"| Wind from the {ds.Currently.WindBearing} " +
            //                                            $"{ds.Currently.WindSpeed}mph (Gusting at {ds.Currently.WindGust}mph)" +
            //                                            $"| Updated: {updated}";
            //                         cmd.Bot.QueueResponse(cmd);
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // }
            // catch (Exception ex)
            // {
            //     cmd.ResponseText = $"error: {ex.Message}";
            //     cmd.Bot.QueueResponse(cmd);
            // }
        }

        /// <summary>
        /// Queues command for the bot to execute or pass on.
        /// </summary>
        /// <param name="cmd">Command object.</param>
        public void QueueCommand(IBotCommand cmd)
        {
            Debug.Assert(cmd != null, "cmd == null");
            Debug.Assert(!string.IsNullOrEmpty(cmd.RequestText), "cmd.RequestText != null/empty");

            this.commandQueue.Enqueue(cmd);
            this.logger?.LogInformation("Queued command");
        }

        /// <summary>
        /// Starts the bot.
        /// </summary>
        /// <returns>Result of the call.</returns>
        public async Task<int> Run()
        {
            this.logger.LogInformation("Gathering system information");
            GetSystemInfo();

            this.logger.LogInformation("Creating any missing resources");
            this.CreateResources();

            this.logger.LogInformation("Loading configuration file");
            this.LoadConfig();

            await this.StartDiscordBots();
            await this.StartIrcBots();
            await this.StartSlackBots();

            this.logger.LogInformation("Juvo is now running");
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
                result.Os = OperatingSystem.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                result.AppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%HOME%/.juvo"));
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
            var appData = this.sysInfo.AppDataPath.FullName;

            Directory.CreateDirectory(appData);
            Directory.CreateDirectory(Path.Combine(appData, "logs"));
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
                this.logger.LogInformation("Loading configuration");
                var json = File.ReadAllText(file);
                this.config = JsonConvert.DeserializeObject<Config>(json);
            }
        }

        private void ProcessCommand(IBotCommand cmd)
        {
            switch (cmd.RequestText.Split(' ')[0].ToLowerInvariant())
            {
                case "ping":
                {
                    cmd.ResponseText = "Pong!";
                    cmd.Bot.QueueResponse(cmd);
                    break;
                }

                case "w":
                case "weather":
                {
                    this.CommandWeather(cmd);
                    break;
                }

                default:
                {
                    // TODO: setting to ignore invalid commands
                    cmd.ResponseText = "Invalid command";
                    cmd.Bot.QueueResponse(cmd);
                    break;
                }
            }
        }

        private async Task StartDiscordBots()
        {
            this.discordBots = new List<DiscordBot>();

            foreach (var disc in this.config?.Discord.Connections)
            {
                this.discordBots.Add(new DiscordBot(this, disc, this.loggerFactory));
            }

            foreach (var bot in this.discordBots)
            {
                await bot.Connect();
            }
        }

        private async Task StartIrcBots()
        {
            this.ircBots = new List<IrcBot>(0);

            foreach (var irc in this.config?.Irc?.Connections)
            {
                this.ircBots.Add(new IrcBot(this, irc, this.loggerFactory));
            }

            // TODO: Connect async?
            this.ircBots.ForEach(bot => bot.Connect());

            await Task.CompletedTask;
        }

        private async Task StartSlackBots()
        {
            this.slackBots = new List<SlackBot>();

            foreach (var slack in this.config?.Slack.Connections)
            {
                this.slackBots.Add(new SlackBot(this, slack, this.loggerFactory));
            }

            foreach (var bot in this.slackBots)
            {
                await bot.Connect();
            }
        }
    }
}
