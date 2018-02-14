// <copyright file="Log4NetLogger.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Logging
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// <see cref="ILogger"/>-compatibe log4net logger.
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        private readonly log4net.ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="log">log4Net log to use.</param>
        public Log4NetLogger(log4net.ILog log)
        {
            this.log = log;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug: return this.log.IsDebugEnabled;
                case LogLevel.Information: return this.log.IsInfoEnabled;
                case LogLevel.Warning: return this.log.IsWarnEnabled;
                case LogLevel.Error: return this.log.IsErrorEnabled;
                case LogLevel.Critical: return this.log.IsFatalEnabled;
                default: throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        /// <inheritdoc/>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentException(nameof(formatter));
            }

            var message = formatter(state, exception);
            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                this.WriteMessage(logLevel, eventId.Id, message, exception);
            }
        }

        private void WriteMessage(LogLevel logLevel, int eventId, string message, Exception exception)
        {
            var evtId = eventId == 0 ? string.Empty : $" [{eventId}]";

            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    this.log.Debug($"{message}{evtId}", exception);
                    break;
                case LogLevel.Information:
                    this.log.Info($"{message}{evtId}", exception);
                    break;
                case LogLevel.Warning:
                    this.log.Warn($"{message}{evtId}", exception);
                    break;
                case LogLevel.Error:
                    this.log.Error($"{message}{evtId}", exception);
                    break;
                case LogLevel.Critical:
                    this.log.Fatal($"{message}{evtId}", exception);
                    break;
                default:
                    this.log.Debug($"{message}{evtId}", exception);
                    break;
            }
        }
    }
}
