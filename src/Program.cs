using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juvo.Net.Irc;

namespace Juvo
{
    public enum OS { Unknown = 0, Windows = 1, Linux = 2, Osx = 3 }

    public class Program
    {
        const string ConfigFileName    = "config.json";
        const string ResourcePrefix    = "src.resources.";
        const int    SchedulerDelay    = 0;
        const int    SchedulerInterval = 1000;

        public static Config         AppConfig;
        public static string         AppDataPath;
        public static OS             CurrentPlatform;
        public static List<IrcBot>   IrcBots;
        public static ILoggerFactory LoggerFactory;
        public static ILogger        Logger;
        public static bool           Quit;
        public static Timer          Scheduler;
        public static object         SchedulerState;

        static void Main(string[] args)
        {
            LoggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Information)
                .AddDebug(LogLevel.Trace);
            Logger = LoggerFactory.CreateLogger<Program>();

            GetOSPlatform();
            //TODO: handle Unknown OS
            Logger.LogInformation($"Platform: {CurrentPlatform}");

            GetpAppDataPath();
            //TODO: handle empty path
            Logger.LogInformation($"App Data: {AppDataPath}");

            CreateAppDataFolders();
            CreateConfigFile();
            LoadConfigFile();
            StartupIrc();

            //var s = new SocketClient();
            //s.Connect("localhost", 8000);
            //s.ConnectCompleted += (o, e) => s.Send("Hello\r\n");


            Quit = false;
            SchedulerState = null;
            Scheduler = new Timer(SchedulerRun, SchedulerState, SchedulerDelay, SchedulerInterval);

            ConsoleKeyInfo key;
            while (!Quit && (key = Console.ReadKey(true)).Key != ConsoleKey.Q)
            {
            }
        }

        public static void SchedulerRun(object state)
        {
            //Logger.Info($"Scheduler run on tid #{Thread.CurrentThread.ManagedThreadId}...");
        }

        static void CreateAppDataFolders()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Logger.LogInformation("Creating application data directory...");
                Directory.CreateDirectory(AppDataPath);
            }
            if (!Directory.Exists(Path.Combine(AppDataPath, "logs")))
            {
                Logger.LogInformation("Creating logs directory");
                Directory.CreateDirectory(Path.Combine(AppDataPath, "logs"));
            }
        }
        static void CreateConfigFile()
        {
            if (!File.Exists(Path.Combine(AppDataPath, ConfigFileName)))
            {
                Logger.LogInformation("Creating configuration file...");
                using (var f = File.Create(Path.Combine(AppDataPath, ConfigFileName)))
                using (var s = typeof(Program).GetTypeInfo().Assembly.GetManifestResourceStream(string.Concat(ResourcePrefix, "config.json")))
                {
                    s.CopyTo(f);
                    f.Flush();
                }
            }
        }
        static void GetpAppDataPath()
        {
            if (CurrentPlatform == OS.Windows)
            { AppDataPath = Environment.ExpandEnvironmentVariables("%APPDATA%/juvo"); }

            if (CurrentPlatform == OS.Linux)
            { AppDataPath = Environment.ExpandEnvironmentVariables("%HOME%/.juvo"); }
        }
        static void GetOSPlatform()
        {
            CurrentPlatform = OS.Unknown;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { CurrentPlatform = OS.Windows; }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { CurrentPlatform = OS.Linux; }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { CurrentPlatform = OS.Osx; }
        }
        static void LoadConfigFile()
        {
            if (File.Exists(Path.Combine(AppDataPath, ConfigFileName)))
            {
                Logger.LogInformation("Loading configuration");
                var json = File.ReadAllText(Path.Combine(AppDataPath, ConfigFileName));
                AppConfig = JsonConvert.DeserializeObject<Config>(json);
            }
        }
        static void StartupIrc()
        {
            IrcBots = new List<IrcBot>(0);
            foreach (var irc in AppConfig.Irc.Connections)
            {
                IrcBots.Add(new IrcBot(irc, LoggerFactory));
            }

            foreach (var bot in IrcBots)
            {
                bot.Connect();
            }
        }
    }
}