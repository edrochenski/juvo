// <copyright file="DiscordBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    /// <summary>
    /// Default Discord Bot factory.
    /// </summary>
    /// <typeparam name="T">Discord bot type.</typeparam>
    public class DiscordBotFactory<T> : IDiscordBotFactory
        where T : IDiscordBot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordBotFactory{T}"/> class.
        /// </summary>
        public DiscordBotFactory()
        {
        }
    }
}
