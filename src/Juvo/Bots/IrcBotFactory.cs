// <copyright file="IrcBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    /// <summary>
    /// Default IRC Bot factory.
    /// </summary>
    /// <typeparam name="T">Discord bot type.</typeparam>
    public class IrcBotFactory<T> : IIrcBotFactory
        where T : IIrcBot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IrcBotFactory{T}"/> class.
        /// </summary>
        public IrcBotFactory()
        {
        }
    }
}
