// <copyright file="Log4NetLoggerProvider.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Logging
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// <see cref="ILoggerProvider"/>-compatible provider for log4net.
    /// </summary>
    public class Log4NetLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ILogger> loggers;
        private string configFileName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLoggerProvider"/> class.
        /// </summary>
        /// <param name="configFileName">Configuration file name.</param>
        public Log4NetLoggerProvider(string configFileName)
        {
            this.loggers = new ConcurrentDictionary<string, ILogger>();
            this.configFileName = configFileName;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return this.loggers.GetOrAdd(categoryName, this.CreateLoggerImplementation);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.loggers.Clear();
        }

        private Log4NetLogger CreateLoggerImplementation(string categoryName)
        {
            var repository = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());

            if (log4net.LogManager.GetCurrentLoggers(repository.Name).Count() == 0)
            {
                log4net.Config.XmlConfigurator.Configure(repository, new FileInfo(this.configFileName));
            }

            var logger = log4net.LogManager.GetLogger(repository.Name, categoryName);
            return new Log4NetLogger(logger);
        }
    }
}
