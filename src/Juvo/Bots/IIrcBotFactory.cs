// <copyright file="IIrcBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Represents a Discord Bot Factory.
    /// </summary>
    public interface IIrcBotFactory
    {
        /// <summary>
        /// Creates a new irc bot instance.
        /// </summary>
        /// <param name="config">Bot's configuration.</param>
        /// <param name="host">Host reference.</param>
        /// <returns>Instance of an <see cref="IIrcBot"/>.</returns>
        IIrcBot Create(IrcConfigConnection config, IJuvoClient host);
    }
}
