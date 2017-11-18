// <copyright file="IBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

// TODO: add Connect()
namespace JuvoProcess
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a Juvo compatible bot.
    /// </summary>
    public interface IBot
    {
        /// <summary>
        /// Gets the type of bot.
        /// </summary>
        BotType Type { get; }

        /// <summary>
        /// Queue response from outside source.
        /// </summary>
        /// <param name="cmd">Command to queue.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        Task QueueResponse(IBotCommand cmd);
    }
}
