using Juvo.Net.Irc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JuvoConsole
{
    public class Juvo
    {
    /*/ Constants /*/
        public const string ConfigFileName = "config.json";
        public const int    TimerTickRate  = 1000;

    /*/ Fields /*/
        readonly ILoggerFactory     loggerFactory;
        readonly ILogger<Juvo>      logger;
        readonly ManualResetEvent   resetEvent;
        readonly SystemInfo         sysInfo;
        readonly Timer              timer;

        Config       config;
        List<IrcBot> ircBots;
        Timer        scheduler;
        DateTime     started;

    /*/ Properties /*/
        public JuvoState State { get; protected set; }

    /*/ *structors /*/
        public Juvo(ILoggerFactory loggerFactory) : this(loggerFactory, null) { }
        public Juvo(ILoggerFactory loggerFactory, ManualResetEvent resetEvent)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.resetEvent    = resetEvent;

            logger  = loggerFactory.CreateLogger<Juvo>();
            started = DateTime.UtcNow;
            State   = JuvoState.Idle;
            sysInfo = GetSystemInfo();
            timer   = new Timer(TimerTick, null, TimerTickRate, TimerTickRate); //TODO: extract constants
        }
    
    /*/ Public Methods /*/
        public async Task<int> Run()
        {
            logger.LogInformation("Gathering system information");
            GetSystemInfo();

            logger.LogInformation("Creating any missing resources");
            CreateResources();

            logger.LogInformation("Loading configuration file");
            LoadConfig();

            await StartIrcBots();



            logger.LogInformation("Juvo is now running");
            State = JuvoState.Running;

            return await Task.FromResult(0);
        }
    
    /*/ Protected Methods /*/
        protected void TimerTick(object state)
        {

        }

    /*/ Private Methods /*/
        private void CreateAppFolders()
        {
            var appData = sysInfo.AppDataPath.FullName;

            Directory.CreateDirectory(appData);
            Directory.CreateDirectory(Path.Combine(appData, "logs"));
        }
        private void CreateConfigFile()
        {
            var appData = sysInfo.AppDataPath.FullName;
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
            CreateAppFolders();
            CreateConfigFile();
        }
        private static string GetDefaultConfig()
        {
            //TODO: decide if we should use a resource instead
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

            //TODO: extract constant path/dir name
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
                //TODO: Set AppDataPath on OSX
                result.Os = OperatingSystem.Osx;
            }

            return result;
        }
        private void LoadConfig()
        {
            var file = Path.Combine(sysInfo.AppDataPath.FullName, ConfigFileName);
            if (File.Exists(file))
            {
                logger.LogInformation("Loading configuration");
                var json = File.ReadAllText(file);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
        }
        private async Task StartIrcBots()
        {
            ircBots = new List<IrcBot>(0);

            foreach (var irc in config?.Irc?.Connections)
            { ircBots.Add(new IrcBot(irc, loggerFactory)); }

            //TODO: Connect async?
            ircBots.ForEach(bot => bot.Connect());

            await Task.CompletedTask;

            return;
        }
    }

    public enum JuvoState { Unknown = 0, Idle = 1, Running = 2, Stopped = 3 }
    public enum OperatingSystem { Unknown = 0, Windows = 1, Linux = 2, Osx = 3 }

    public struct SystemInfo
    {
        public DirectoryInfo   AppDataPath;
        public OperatingSystem Os;
    }
}
