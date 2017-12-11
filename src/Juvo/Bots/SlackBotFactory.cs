// <copyright file="SlackBotFactory.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    /// <summary>
    /// Default Slack Bot factory.
    /// </summary>
    /// <typeparam name="T">Discord bot type.</typeparam>
    public class SlackBotFactory<T> : ISlackBotFactory
        where T : ISlackBot
    {
    }
}
