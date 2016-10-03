using juvo.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
/*
TODO:
- Logging? Console, File,
- Clients
- IRC
- Slack
- Messaging
- Email?
- Extensibility
- Scripts
- Plug-ins
*/
namespace juvo
{
    public class Program
    {
        const string ConfigFileName = "config.json";
        const string ResourcePrefix = "src.resources.";
        const int SchedulerDelay    = 0;
        const int SchedulerInterval = 1000;

        public static readonly ILogger Logger = new ConsoleLogger();

        public static string AppDataPath;
        public static OS     CurrentPlatform;
        public static IrcBot IrcBot;
        public static bool   Quit;
        public static Timer  Scheduler;
        public static object SchedulerState;

        public static void Main(string[] args)
        {
            GetOSPlatform();
            //TODO: handle Unknown OS
            Console.WriteLine($"Platform: {CurrentPlatform}");

            GetpAppDataPath();
            //TODO: handle empty path
            Console.WriteLine($"App Data: {AppDataPath}");

            CreateAppDataPath();

            CreateConfigFile();

            //System.IO.File.CreateText("juvo.config.json");
            //System.IO.Directory.CreateDirectory();

            return;
            Logger.Info("Connecting to IRC");
            IrcBot.ConnectAsync("us.undernet.org").Wait();

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

        static void CreateAppDataPath()
        {
            if (!Directory.Exists(AppDataPath))
            {
                Console.WriteLine("Creating application directory...");
                Directory.CreateDirectory(AppDataPath);
            }
        }
        static void CreateConfigFile()
        {
            if (!File.Exists(Path.Combine(AppDataPath, ConfigFileName)))
            {
                Console.WriteLine("Creating configuration file...");
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
    }

    public enum OS
    {
        Unknown = 0,
        Windows = 1,
        Linux   = 2,
        Osx     = 3
    }
}
