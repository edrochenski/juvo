using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace juvo.Logging
{
    public interface ILogger
    {
        void Debug(string format, params object[] args);
        void Error(string format, params object[] args);
        void Fatal(string format, params object[] args);
        void Info(string format, params object[] args);
        void Trace(string format, params object[] args);
        void Warn(string format, params object[] args);

        void Log(LogLevel level, string format, params object[] args);
    }
}
