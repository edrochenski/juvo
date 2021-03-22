// <copyright file="IJuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Representation of a Juvo Client object.
    /// </summary>
    public interface IJuvoClient
    {
        /// <summary>
        /// Gets a list of bots running on the client.
        /// </summary>
        List<IBot> Bots { get; }

        /// <summary>
        /// Gets the config.
        /// </summary>
        Config Config { get; }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        IHttpClient HttpClient { get; }

        /// <summary>
        /// Logs a message to all available log outputs.
        /// </summary>
        /// <param name="level">Level to use.</param>
        /// <param name="message">Message to write.</param>
        /// <param name="exception">Exception to include.</param>
        void Log(LogLevel level, object message, Exception? exception = null);

        /// <summary>
        /// Logs a critical message to all available log outputs.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exception">Exception to include.</param>
        void LogCritical(object message, Exception? exception) => this.Log(LogLevel.Critical, message, exception);

        /// <summary>
        /// Logs a debug message to all available log outputs.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void LogDebug(object message) => this.Log(LogLevel.Debug, message);

        /// <summary>
        /// Logs an error to all available log outputs.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exception">Exception to include.</param>
        void LogError(object message, Exception? exception) => this.Log(LogLevel.Error, message, exception);

        /// <summary>
        /// Logs an informational message to all available log outputs.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void LogInfo(object message) => this.Log(LogLevel.Information, message);

        /// <summary>
        /// Logs a trace message to all available log outputs.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void LogTrace(object message) => this.Log(LogLevel.Trace, message);

        /// <summary>
        /// Logs a warning to all available log outputs.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void LogWarn(object message) => this.Log(LogLevel.Warning, message);

        /// <summary>
        /// Queues a command for the bot to execute.
        /// </summary>
        /// <param name="cmd">Command to execute.</param>
        void QueueCommand(IBotCommand cmd);

        /// <summary>
        /// Queues a response for a specific bot.
        /// </summary>
        /// <param name="cmd">Command containing the source and response.</param>
        /// <returns>Result of the call.</returns>
        Task QueueResponse(IBotCommand cmd);

        /// <summary>
        /// Starts the bot.
        /// </summary>
        /// <returns>Result of the call.</returns>
        Task Run();
    }
}
