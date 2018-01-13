// <copyright file="LogProxy.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using log4net.Core;

    /// <summary>
    /// Log proxy for log4net.
    /// </summary>
    public class LogProxy : ILog
    {
        private readonly log4net.ILog logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogProxy"/> class.
        /// </summary>
        /// <param name="logger">Log4Net logger.</param>
        public LogProxy(log4net.ILog logger) => this.logger = logger;

        /// <inheritdoc/>
        public bool IsDebugEnabled => this.logger.IsDebugEnabled;

        /// <inheritdoc/>
        public bool IsInfoEnabled => this.logger.IsInfoEnabled;

        /// <inheritdoc/>
        public bool IsWarnEnabled => this.logger.IsWarnEnabled;

        /// <inheritdoc/>
        public bool IsErrorEnabled => this.logger.IsErrorEnabled;

        /// <inheritdoc/>
        public bool IsFatalEnabled => this.logger.IsFatalEnabled;

        /// <inheritdoc/>
        public ILogger Logger => this.logger.Logger;

        /// <inheritdoc/>
        public void Debug(object message) => this.logger.Debug(message);

        /// <inheritdoc/>
        public void Debug(object message, Exception exception) => this.logger.Debug(message, exception);

        /// <inheritdoc/>
        public void DebugFormat(string format, params object[] args) => this.logger.DebugFormat(format, args);

        /// <inheritdoc/>
        public void DebugFormat(string format, object arg0) => this.logger.DebugFormat(format, arg0);

        /// <inheritdoc/>
        public void DebugFormat(string format, object arg0, object arg1) => this.logger.DebugFormat(format, arg0, arg1);

        /// <inheritdoc/>
        public void DebugFormat(string format, object arg0, object arg1, object arg2) => this.logger.DebugFormat(format, arg0, arg1, arg2);

        /// <inheritdoc/>
        public void DebugFormat(IFormatProvider provider, string format, params object[] args) => this.logger.DebugFormat(provider, format, args);

        /// <inheritdoc/>
        public void Error(object message) => this.logger.Error(message);

        /// <inheritdoc/>
        public void Error(object message, Exception exception) => this.logger.Debug(message, exception);

        /// <inheritdoc/>
        public void ErrorFormat(string format, params object[] args) => this.logger.ErrorFormat(format, args);

        /// <inheritdoc/>
        public void ErrorFormat(string format, object arg0) => this.logger.DebugFormat(format, arg0);

        /// <inheritdoc/>
        public void ErrorFormat(string format, object arg0, object arg1) => this.logger.ErrorFormat(format, arg0, arg1);

        /// <inheritdoc/>
        public void ErrorFormat(string format, object arg0, object arg1, object arg2) => this.logger.ErrorFormat(format, arg0, arg1, arg2);

        /// <inheritdoc/>
        public void ErrorFormat(IFormatProvider provider, string format, params object[] args) => this.logger.ErrorFormat(provider, format, args);

        /// <inheritdoc/>
        public void Fatal(object message) => this.logger.Fatal(message);

        /// <inheritdoc/>
        public void Fatal(object message, Exception exception) => this.logger.Fatal(message, exception);

        /// <inheritdoc/>
        public void FatalFormat(string format, params object[] args) => this.logger.FatalFormat(format, args);

        /// <inheritdoc/>
        public void FatalFormat(string format, object arg0) => this.logger.FatalFormat(format, arg0);

        /// <inheritdoc/>
        public void FatalFormat(string format, object arg0, object arg1) => this.logger.FatalFormat(format, arg0, arg1);

        /// <inheritdoc/>
        public void FatalFormat(string format, object arg0, object arg1, object arg2) => this.logger.FatalFormat(format, arg0, arg1, arg2);

        /// <inheritdoc/>
        public void FatalFormat(IFormatProvider provider, string format, params object[] args) => this.logger.FatalFormat(provider, format, args);

        /// <inheritdoc/>
        public void Info(object message) => this.logger.Info(message);

        /// <inheritdoc/>
        public void Info(object message, Exception exception) => this.logger.Info(message, exception);

        /// <inheritdoc/>
        public void InfoFormat(string format, params object[] args) => this.logger.InfoFormat(format, args);

        /// <inheritdoc/>
        public void InfoFormat(string format, object arg0) => this.logger.InfoFormat(format, arg0);

        /// <inheritdoc/>
        public void InfoFormat(string format, object arg0, object arg1) => this.logger.InfoFormat(format, arg0, arg1);

        /// <inheritdoc/>
        public void InfoFormat(string format, object arg0, object arg1, object arg2) => this.logger.InfoFormat(format, arg0, arg1, arg2);

        /// <inheritdoc/>
        public void InfoFormat(IFormatProvider provider, string format, params object[] args) => this.logger.InfoFormat(provider, format, args);

        /// <inheritdoc/>
        public void Warn(object message) => this.logger.Warn(message);

        /// <inheritdoc/>
        public void Warn(object message, Exception exception) => this.logger.Warn(message, exception);

        /// <inheritdoc/>
        public void WarnFormat(string format, params object[] args) => this.logger.WarnFormat(format, args);

        /// <inheritdoc/>
        public void WarnFormat(string format, object arg0) => this.logger.WarnFormat(format, arg0);

        /// <inheritdoc/>
        public void WarnFormat(string format, object arg0, object arg1) => this.logger.WarnFormat(format, arg0, arg1);

        /// <inheritdoc/>
        public void WarnFormat(string format, object arg0, object arg1, object arg2) => this.logger.WarnFormat(format, arg0, arg1, arg2);

        /// <inheritdoc/>
        public void WarnFormat(IFormatProvider provider, string format, params object[] args) => this.logger.WarnFormat(provider, format, args);
    }
}
