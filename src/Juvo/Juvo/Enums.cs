// <copyright file="Enums.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Juvo
{
    /// <summary>
    /// User identity type.
    /// </summary>
    public enum UserIdentityType
    {
        /// <summary>
        /// None or unknown.
        /// </summary>
        None,

        /// <summary>
        /// Default.
        /// </summary>
        Default,

        /// <summary>
        /// Irc.
        /// </summary>
        Irc,

        /// <summary>
        /// Discord.
        /// </summary>
        Discord,

        /// <summary>
        /// Slack.
        /// </summary>
        Slack
    }
}
