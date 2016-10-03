using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace juvo.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Debug(string format, params object[] args)
        { Log(LogLevel.Debug, format, args); }
        public void Error(string format, params object[] args)
        { Log(LogLevel.Error, format, args); }
        public void Fatal(string format, params object[] args)
        { Log(LogLevel.Fatal, format, args); }
        public void Info(string format, params object[] args)
        { Log(LogLevel.Info, format, args); }
        public void Log(LogLevel level, string format, params object[] args)
        {
            Console.Write(DateTime.Now.ToString("HH:mm:ss.fff"));

            switch (level)
            {
                case LogLevel.Fatal: Console.ForegroundColor = ConsoleColor.DarkRed; break;
                case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogLevel.Warn:  Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogLevel.Info:  Console.ForegroundColor = ConsoleColor.Green; break;
                case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.White; break;
                case LogLevel.Trace: Console.ForegroundColor = ConsoleColor.Gray; break;
            }
            Console.Write(" {0,-5} ", level.ToString().ToUpperInvariant());
            Console.ResetColor();

            Console.WriteLine(format, args);
        }
        public void Trace(string format, params object[] args)
        { Log(LogLevel.Trace, format, args); }
        public void Warn(string format, params object[] args)
        { Log(LogLevel.Warn, format, args); }
    }
}
