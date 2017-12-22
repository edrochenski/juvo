// <copyright file="ChannelType.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Channel type.
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// Direct Message
        /// </summary>
        DirectMessage = 1,

        /// <summary>
        /// Group Direct Message
        /// </summary>
        GroupDirectMessage = 3,

        /// <summary>
        /// Guild Category
        /// </summary>
        GuildCategory = 4,

        /// <summary>
        /// Guild Text
        /// </summary>
        GuildText = 0,

        /// <summary>
        /// Guild Voice
        /// </summary>
        GuildVoice = 2
    }
}
