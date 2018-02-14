// <copyright file="Enums.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    /// <summary>
    /// Source type the command originated from
    /// </summary>
    public enum CommandSourceType
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// No source type.
        /// </summary>
        None = 0,

        /// <summary>
        /// A channel, group, or other source that contains multiple members.
        /// </summary>
        ChannelOrGroup = 1,

        /// <summary>
        /// A message sent directly to the bot.
        /// </summary>
        Message = 2
    }
}
