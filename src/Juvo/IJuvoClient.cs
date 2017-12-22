// <copyright file="IJuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using JuvoProcess.Bots;

    /// <summary>
    /// Representation of a Juvo Client object.
    /// </summary>
    public interface IJuvoClient
    {
        /// <summary>
        /// Queues a command for the bot to execute.
        /// </summary>
        /// <param name="cmd">Command to execute.</param>
        void QueueCommand(IBotCommand cmd);
    }
}
