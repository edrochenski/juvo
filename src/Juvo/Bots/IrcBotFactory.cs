// <copyright file="IrcBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using JuvoProcess.Configuration;

    /// <summary>
    /// Default IRC Bot factory.
    /// </summary>
    public class IrcBotFactory : IIrcBotFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IrcBotFactory"/> class.
        /// </summary>
        public IrcBotFactory()
        {
        }

        /// <inheritdoc />
        public IIrcBot Create(IrcConfigConnection config, IJuvoClient host)
        {
            return new IrcBot(config, host);
        }
    }
}
