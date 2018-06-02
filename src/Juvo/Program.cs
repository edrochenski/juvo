// <copyright file="Program.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

[assembly: System.Resources.NeutralResourcesLanguage("en")]
namespace JuvoProcess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Newtonsoft.Json;

    /// <summary>
    /// Main class for the assembly, contains entry point.
    /// </summary>
    public class Program
    {
        /*/ Fields /*/

        private static readonly string DefaultConfigFileName = "config.json";
        private static readonly ManualResetEvent ResetEvent;
        private static readonly IServiceCollection ServiceCollection;

        private static Program instance;
        private static IServiceProvider serviceProvider;

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
        public static Program Instance => instance;

        /// <summary>
        /// Gets the instances service provider.
        /// </summary>
        public IServiceProvider Services => Program.serviceProvider;

        /*/ Methods /*/

        /// <summary>
        /// Runs the Juvo process.
        /// </summary>
        /// <returns>A Task object associated with the async operation.</returns>
        public async Task Run()
        {
            await this?.juvoClient.Run();
        }

        private static IWebHost BuildWebHost()
        {
            var builder = new WebHostBuilder();
            return builder
                .UseContentRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"))
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000);
                })
                .Configure(cfg =>
                {
                    cfg.UseDefaultFiles(new DefaultFilesOptions
                    {
                        DefaultFileNames = new List<string> { "index.html" },
                        FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "wwwroot")),
                        RequestPath = string.Empty
                    });
                    cfg.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "wwwroot")),
                        RequestPath = string.Empty
                    });
                })
                .Build();
        }

        private static SystemInfo GetSystemInfo()
        {
            var result = new SystemInfo { AppDataPath = null, Os = OperatingSystem.Unknown };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                result.AppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%APPDATA%/juvo"));
                result.LocalAppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/juvo"));
                result.Os = OperatingSystem.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                result.AppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("~/.config/.juvo"));
                result.LocalAppDataPath = new DirectoryInfo(Environment.ExpandEnvironmentVariables("~/.config/.juvo"));
                result.Os = OperatingSystem.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                result.AppDataPath =
                    new DirectoryInfo(Environment.ExpandEnvironmentVariables("~/Library/Preferences/juvo"));
                result.LocalAppDataPath =
                    new DirectoryInfo(Environment.ExpandEnvironmentVariables("~/Library/Application Support/"));
                result.Os = OperatingSystem.Osx;
            }

            return result;
        }

        private static Config LoadConfiguration()
        {
            var sysInfo = GetSystemInfo();

            var file = Path.Combine(sysInfo.AppDataPath.FullName, DefaultConfigFileName);

            if (!File.Exists(file))
            {
                Environment.Exit(-1);
            }

            var json = File.ReadAllText(file);
            var config = JsonConvert.DeserializeObject<Config>(json);
            config.System = sysInfo;

            if (config == null)
            {
                Environment.Exit(-1);
            }

            return config;
        }

        private static void Main(string[] args)
        {
            SetupDependencies();

            instance = new Program(
                serviceProvider.GetService<IJuvoClient>(),
                serviceProvider.GetService<ILogManager>());

            instance.juvoClient.Run().Wait();
            WaitHandle.WaitAll(new[] { ResetEvent });
        }

        private static void SetupDependencies()
        {
            ServiceCollection.AddSingleton<ILogManager>(sp =>
            {
                var logCfg = new FileInfo("log4net.config");
                var logMgr = LogManager.GetRepository(Assembly.GetEntryAssembly());

                XmlConfigurator.ConfigureAndWatch(logMgr, logCfg);
                return new LogManagerProxy();
            });
            ServiceCollection.AddSingleton(ResetEvent);
            ServiceCollection.AddSingleton<IStorageHandler, StorageHandler>();
            ServiceCollection.AddSingleton<IJuvoClient, JuvoClient>();
            ServiceCollection.AddSingleton<IDiscordBotFactory, DiscordBotFactory>();
            ServiceCollection.AddSingleton<IIrcBotFactory, IrcBotFactory>();
            ServiceCollection.AddSingleton<ISlackBotFactory, SlackBotFactory>();
            ServiceCollection.AddTransient<IDiscordClient, DiscordClient>();
            ServiceCollection.AddTransient<IIrcClient, IrcClient>();
            ServiceCollection.AddTransient<ISlackClient, SlackClient>();
            ServiceCollection.AddTransient<ISocketClient, SocketClient>();
            ServiceCollection.AddTransient<ISocket, SocketProxy>();
            ServiceCollection.AddTransient<IClientWebSocket, ClientWebSocketProxy>();
            ServiceCollection.AddTransient<IHttpClient, HttpClientProxy>();
            ServiceCollection.AddTransient<HttpMessageHandler, HttpClientHandlerProxy>();
            ServiceCollection.AddTransient(sp => LoadConfiguration());
            ServiceCollection.AddTransient(sp => BuildWebHost());

            serviceProvider = ServiceCollection.BuildServiceProvider();
        }
    }
}