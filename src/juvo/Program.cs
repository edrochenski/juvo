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
        private static readonly ManualResetEvent ResetEvent;

/*/ Constructors /*/
        static Program()
        {
            // GlobalContext.Properties["juvo_appdata"] =
            XmlConfigurator.ConfigureAndWatch(
                LogManager.GetRepository(Assembly.GetEntryAssembly()),
                new FileInfo("log4net.config"));
            Log = LogManager.GetLogger(typeof(Program));

            var report = DiagnosticReport.Generate();
            Log.Debug($"Diagnostic report:{Environment.NewLine}{report}");

            ResetEvent = new ManualResetEvent(false);
            Juvo = new JuvoClient(
                new DiscordBotFactory(),
                new IrcBotFactory(),
                new SlackBotFactory(),
                new HttpClientProxy(new HttpClientHandler()),
                new ClientWebSocketProxy(),
                ResetEvent);
        }

/*/ Methods /*/
        private static void Main(string[] args)
        {
            Log.Info("Attempting to launch Juvo...");
            Juvo.Run().Wait();

            WaitHandle.WaitAll(new[] { ResetEvent });
        }
    }
}