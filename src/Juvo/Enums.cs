// <copyright file="Enums.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    /// <summary>
    /// Type of bot.
    /// </summary>
    public enum BotType
    {
        /// <summary>
        /// None or unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// IRC bot.
        /// </summary>
        Irc = 1,

        /// <summary>
        /// Slack bot.
        /// </summary>
        Slack = 2,

        /// <summary>
        /// Discord bot.
        /// </summary>
        Discord = 3
    }

    /// <summary>
    /// Juvo state.
    /// </summary>
    public enum JuvoState
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Idle state.
        /// </summary>
        Idle = 1,

        /// <summary>
        /// Running state.
        /// </summary>
        Running = 2,

        /// <summary>
        /// Stopped state.
        /// </summary>
        Stopped = 3
    }
}
