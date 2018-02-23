// <copyright file="IBotPlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Bots
{
    using System.Collections.Generic;

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
        /// Test method.
        /// </summary>
        /// <param name="cmd">Command to execute.</param>
        /// <returns>Updated command object.</returns>
        IBotCommand Execute(IBotCommand cmd);
    }
}
