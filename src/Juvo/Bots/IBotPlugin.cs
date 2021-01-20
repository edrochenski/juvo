// <copyright file="IBotPlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Bots
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a bot plugin.
    /// </summary>
    public interface IBotPlugin
    {
        /// <summary>
        /// Gets the collection of commands for the plugin.
        /// </summary>
        IList<string> Commands { get; }

        /// <summary>
        /// Gets the collection of times (minutes) to have the bot execute a command.
        /// </summary>
        IList<int>? CommandTimeMin { get; } // nocommit: need to rethink name and process a bit?

        /// <summary>
        /// Test method.
        /// </summary>
        /// <param name="cmd">Command to execute.</param>
        /// <param name="client">Host client of the plugin.</param>
        /// <returns>Updated command object.</returns>
        Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient client);
    }
}
