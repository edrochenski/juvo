// <copyright file="Program.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System.Threading;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Main class for the assembly, contains entry point.
    /// </summary>
    public class Program
    {
/*/ Fields /*/
        private static readonly JuvoClient Juvo;
        private static readonly ILoggerFactory LoggerFactory;
        private static readonly ILogger<Program> Logger;
        private static readonly ManualResetEvent ResetEvent;

/*/ Constructors /*/
        static Program()
        {
            LoggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Trace)
                .AddDebug(LogLevel.Trace);
            Logger = LoggerFactory.CreateLogger<Program>();
            ResetEvent = new ManualResetEvent(false);
            Juvo = new JuvoClient(LoggerFactory, ResetEvent);
        }

/*/ Methods /*/
        private static void Main(string[] args)
        {
            Logger.LogInformation("Attempting to launch Juvo...");
            Juvo.Run().Wait();

            WaitHandle.WaitAll(new[] { ResetEvent });
        }
    }
}