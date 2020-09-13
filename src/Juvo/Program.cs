// <copyright file="Program.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
[assembly: System.Resources.NeutralResourcesLanguage("en")]

namespace JuvoProcess
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.IO;
    using JuvoProcess.Net;
    using JuvoProcess.Net.Discord;
    using JuvoProcess.Net.Irc;
    using JuvoProcess.Net.Slack;
    using log4net;
    using log4net.Config;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// Main "executing" class for the assembly, contains entry point and handles
    /// passing options and dependencies to <see cref="JuvoClient"/> instance.
    /// </summary>
    public class Program
    {
        /*/ Fields /*/

        private static readonly string DefaultConfigFileName = "config.json";
        private static readonly ManualResetEvent ResetEvent;
        private static readonly IServiceCollection ServiceCollection;
        private static IServiceProvider? serviceProvider;

        private readonly IJuvoClient juvoClient;
        private readonly ILog log;
        private readonly ILogManager logManager;

        /*/ Constructors /*/

        static Program()
        {
            ServiceCollection = new ServiceCollection();
            ResetEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="juvoClient">Client to use.</param>
        /// <param name="logManager">Log Manager to use.</param>
        public Program(IJuvoClient juvoClient, ILogManager logManager)
        {
            this.logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            this.juvoClient = juvoClient ?? throw new ArgumentNullException(nameof(juvoClient));

            this.log = this.logManager.GetLogger(typeof(Program));
        }

        /*/ Properties /*/

        /// <summary>
        /// Gets the running instance of the process.
        /// </summary>
        public static Program? Instance { get; private set; }

        /// <summary>
        /// Gets the instances service provider.
        /// </summary>
        public IServiceProvider? Services => serviceProvider;

        /*/ Methods /*/

        private static string GetDefaultConfigFile()
        {
            string? path = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Environment.ExpandEnvironmentVariables("%APPDATA%/juvo");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = Environment.ExpandEnvironmentVariables("%HOME%/.config/juvo");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = Environment.ExpandEnvironmentVariables("%HOME%/Library/Preferences/juvo");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unknown OS: Could not determine default configuration path");
                Environment.Exit(-1);
            }

            return Path.Combine(path, DefaultConfigFileName);
        }

        private static Config LoadConfiguration()
        {
            var file = GetDefaultConfigFile();

            if (!File.Exists(file))
            {
                Console.WriteLine($"Unable to find config '{file}'");
                Environment.Exit(-1);
            }

            var json = File.ReadAllText(file);
            var config = JsonConvert.DeserializeObject<Config>(json);

            if (config == null)
            {
                Console.WriteLine($"Unable to load config '{file}'");
                Environment.Exit(-1);
            }

            return config;
        }

        private static void Main(string[] args)
        {
            SetupDependencies();

            Instance = new Program(
                serviceProvider.GetService<IJuvoClient>(),
                serviceProvider.GetService<ILogManager>());

            Instance.juvoClient.Run().Wait();
            WaitHandle.WaitAll(new[] { ResetEvent });
        }

        private static void SetupDependencies()
        {
            serviceProvider = ServiceCollection
                .AddSingleton<ILogManager>(sp =>
                {
                    var logCfg = new FileInfo("log4net.config");
                    var logMgr = LogManager.GetRepository(Assembly.GetEntryAssembly());

                    XmlConfigurator.ConfigureAndWatch(logMgr, logCfg);
                    return new LogManagerProxy();
                })
                .AddSingleton(ResetEvent)
                .AddSingleton<IStorageHandler, StorageHandler>()
                .AddSingleton<IJuvoClient, JuvoClient>()
                .AddSingleton<IDiscordBotFactory, DiscordBotFactory>()
                .AddSingleton<IIrcBotFactory, IrcBotFactory>()
                .AddSingleton<ISlackBotFactory, SlackBotFactory>()
                .AddTransient<IDiscordClient, DiscordClient>()
                .AddTransient<IIrcClient, IrcClient>()
                .AddTransient<ISlackClient, SlackClient>()
                .AddTransient<ISocketClient, SocketClient>()
                .AddTransient<ISocket, SocketProxy>()
                .AddTransient<IClientWebSocket, ClientWebSocketProxy>()
                .AddTransient<IHttpClient, HttpClientProxy>()
                .AddTransient<IWebHostBuilder, WebHostBuilder>()
                .AddTransient<HttpMessageHandler, HttpClientHandlerProxy>()
                .AddTransient(sp => LoadConfiguration())
                .BuildServiceProvider();
        }
    }
}