// <copyright file="Program.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using Easy.Common;
    using JuvoProcess.Bots;
    using JuvoProcess.Net;
    using log4net;
    using log4net.Config;

    /// <summary>
    /// Main class for the assembly, contains entry point.
    /// </summary>
    public class Program
    {
/*/ Fields /*/

    // PUBLIC

        /// <summary>
        /// Juvo client instance.
        /// </summary>
        public static readonly JuvoClient Juvo;

    // PRIVATE
        private static readonly ILog Log;
        private static readonly ILogManager LogMgr;
        private static readonly ManualResetEvent ResetEvent;
        private static readonly IWebHost WebServer;
        private static readonly CancellationToken WebHostToken;

/*/ Constructors /*/
        static Program()
        {
            // GlobalContext.Properties["juvo_appdata"] =
            XmlConfigurator.ConfigureAndWatch(
                LogManager.GetRepository(Assembly.GetEntryAssembly()),
                new FileInfo("log4net.config"));
            LogMgr = new LogManagerProxy();
            Log = LogMgr.GetLogger(typeof(Program));
            WebHostToken = default(CancellationToken);
            WebServer = BuildWebHost();
            WebServer.RunAsync(WebHostToken);

            // var report = DiagnosticReport.Generate();
            // Log.Debug($"Diagnostic report:{Environment.NewLine}{report}");
            ResetEvent = new ManualResetEvent(false);
            Juvo = new JuvoClient(
                new DiscordBotFactory(LogMgr),
                new IrcBotFactory(LogMgr),
                new SlackBotFactory(LogMgr),
                LogMgr,
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

        private static void Main(string[] args)
        {
            Log.Info("Attempting to launch Juvo...");
            Juvo.Run().Wait();

            WaitHandle.WaitAll(new[] { ResetEvent });
        }
    }
}