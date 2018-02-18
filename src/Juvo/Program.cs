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
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.Logging;
    using JuvoProcess.Resources.Logging;
    using log4net;
    using log4net.Config;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Main class for the assembly, contains entry point.
    /// </summary>
    public class Program
    {
/*/ Fields /*/

    // PUBLIC

        /// <summary>
        /// Default file name for the configuration.
        /// </summary>
        public const string DefaultConfigFileName = "config.json";

        /// <summary>
        /// Juvo client instance.
        /// </summary>
        public static readonly JuvoClient Juvo;

    // PRIVATE
        private static readonly ILog Log;
        private static readonly ILogManager LogMgr;
        private static readonly ManualResetEvent ResetEvent;

/*/ Constructors /*/
        static Program()
        {
            var logCfg = new FileInfo("log4net.config");
            var logMgr = LogManager.GetRepository(Assembly.GetEntryAssembly());

            XmlConfigurator.ConfigureAndWatch(logMgr, logCfg);
            LogMgr = new LogManagerProxy();
            Log = LogMgr.GetLogger(typeof(Program));

            // var report = DiagnosticReport.Generate();
            // Log.Debug($"Diagnostic report:{Environment.NewLine}{report}");
            ResetEvent = new ManualResetEvent(false);
            Juvo = new JuvoClient(
                LoadConfiguration(),
                new DiscordBotFactory(LogMgr),
                new IrcBotFactory(LogMgr),
                new SlackBotFactory(LogMgr),
                LogMgr,
                BuildWebHost(),
                ResetEvent);
        }

        /*/ Methods /*/
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
                .ConfigureServices(cfg =>
                {
                    cfg.AddLogging();
                })
                .ConfigureLogging(cfg =>
                {
                    cfg.AddProvider(new Log4NetLoggerProvider(null));
                    cfg.SetMinimumLevel(LogLevel.Trace);
                })
                .Build();
        }

        private static void CreateConfigFile()
        {
            // var appData = this.config.System.AppDataPath.FullName;
            // if (!File.Exists(Path.Combine(appData, ConfigFileName)))
            // {
            //     using (var file = File.CreateText(Path.Combine(appData, ConfigFileName)))
            //     {
            //         file.Write(GetDefaultConfig());
            //         file.Flush();
            //     }
            // }
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

        private static Config LoadConfiguration()
        {
            Log?.Info("Loading configuration");

            var sysInfo = GetSystemInfo();

            var file = Path.Combine(sysInfo.AppDataPath.FullName, DefaultConfigFileName);

            if (!File.Exists(file))
            {
                Log?.Error($"Configuration file is missing ({file})");
                Environment.Exit(-1);
            }

            var json = File.ReadAllText(file);
            var config = JsonConvert.DeserializeObject<Config>(json);
            config.System = sysInfo;

            if (config == null)
            {
                Log?.Error($"Configuration file could not be loaded (length: {json.Length})");
                Environment.Exit(-1);
            }

            return config;
        }

        private static void Main(string[] args)
        {
            Log.Info(InfoResx.LaunchingJuvo);
            Juvo.Run().Wait();

            WaitHandle.WaitAll(new[] { ResetEvent });
        }
    }
}