// <copyright file="IDiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using JuvoProcess.Configuration;

    /// <summary>
    /// Represents a Discord Bot Factory.
    /// </summary>
    public interface IDiscordBotFactory
    {
        /// <summary>
        /// Creates a new discord bot instance.
        /// </summary>
        /// <param name="config">Bot's configuration.</param>
        /// <param name="services">Service provide.</param>
        /// <param name="juvoClient">Host.</param>
        /// <returns>Instance of an <see cref="IDiscordBot"/>.</returns>
        IDiscordBot Create(DiscordConfigConnection config, IServiceProvider services, IJuvoClient juvoClient);
    }
}
