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
