// <copyright file="IJuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using JuvoProcess.Bots;
    using JuvoProcess.Net;
    using log4net;

    /// <summary>
    /// Representation of a Juvo Client object.
    /// </summary>
    public interface IJuvoClient
    {
        /// <summary>
        /// Gets the log.
        /// </summary>
        ILog Log { get; }

        /// <summary>
        /// Gets the Http Client.
        /// </summary>
        IHttpClient HttpClient { get; }

        /// <summary>
        /// Queues a command for the bot to execute.
        /// </summary>
        /// <param name="cmd">Command to execute.</param>
        void QueueCommand(IBotCommand cmd);
    }
}
