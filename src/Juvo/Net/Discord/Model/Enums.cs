// <copyright file="Enums.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Content filter level.
    /// </summary>
    /// <remarks>
    /// https://discordapp.com/developers/docs/resources/guild#guild-object-explicit-content-filter-level
    /// </remarks>
    public enum ExplicitContentFilterLevel
    {
        /// <summary>
        /// Disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// Memebers without roles.
        /// </summary>
        MembersWithoutRoles,

        /// <summary>
        /// All members.
        /// </summary>
        AllMembers
    }

    /// <summary>
    /// Notification levels.
    /// </summary>
    /// <remarks>
    /// https://discordapp.com/developers/docs/resources/guild#guild-object-default-message-notification-level
    /// </remarks>
    public enum MessageNotificationLevel
    {
        /// <summary>
        /// All messages.
        /// </summary>
        AllMessages,

        /// <summary>
        /// Only mentions.
        /// </summary>
        OnlyMentions
    }

    /// <summary>
    /// MFA level.
    /// </summary>
    public enum MfaLevel
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Elevated.
        /// </summary>
        Elevated
    }

    /// <summary>
    /// Represents a verification level.
    /// </summary>
    /// <remarks>
    /// https://discordapp.com/developers/docs/resources/guild#guild-object-verification-level
    /// </remarks>
    public enum VerificationLevel
    {
        /// <summary>
        /// Unrestricted.
        /// </summary>
        None,

        /// <summary>
        /// Must have verified email on account.
        /// </summary>
        Low,

        /// <summary>
        /// Must be registered on Discord for longer then 5 minutes.
        /// </summary>
        Medium,

        /// <summary>
        /// (╯°□°）╯︵ ┻━┻ - must be a member of the server for longer than 10 minutes
        /// </summary>
        High,

        /// <summary>
        /// ┻━┻ミヽ(ಠ益ಠ)ﾉ彡┻━┻ - must have a verified phone number
        /// </summary>
        VeryHigh
    }
}
