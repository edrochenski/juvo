using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace juvo.Logging
{
    public class LogEvent
    {
        Exception exception;
        LogLevel level;
        string message;

        public Exception Exception { get { return exception; } }
        public LogLevel Level { get { return level; } }
        public string Message { get { return message; } }

        public LogEvent(string message)
            : this(LogLevel.Default, message) { }
        public LogEvent(string format, params object[] args)
            : this(LogLevel.Default, format, args) { }
        public LogEvent(Exception exception)
            : this(LogLevel.Error, exception) { }
        public LogEvent(LogLevel level, Exception exception)
        { Initialize(level, exception, exception.Message); }
        public LogEvent(LogLevel level, string message)
        { Initialize(level, null, message); }
        public LogEvent(LogLevel level, string format, params object[] args)
            : this(level, String.Format(format, args)) { }

        private void Initialize(LogLevel level, Exception exception, string message)
        {
            this.exception = exception;
            this.level = level;
            this.message = message;
        }
    }
}
