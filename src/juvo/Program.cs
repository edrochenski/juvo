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

namespace JuvoConsole
{

    public class Program
    {
        public readonly static Juvo             Juvo;
        public readonly static ILoggerFactory   LoggerFactory;
        public readonly static ILogger<Program> Logger;
        public readonly static ManualResetEvent ResetEvent;

        static Program()
        {
            LoggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Information)
                .AddDebug(LogLevel.Trace);
            Logger = LoggerFactory.CreateLogger<Program>();
            ResetEvent = new ManualResetEvent(false);

            Juvo = new Juvo(LoggerFactory, ResetEvent);
        }

        static void Main(string[] args)
        {
            Logger.LogInformation("Attempting to launch Juvo...");
            Juvo.Run().Wait();

            WaitHandle.WaitAll(new[] { ResetEvent });
        }
    }
}