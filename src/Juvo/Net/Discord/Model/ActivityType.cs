// <copyright file="ActivityType.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Discord.Model
{
    /// <summary>
    /// Activity types that a user can be involved in.
    /// </summary>
    public enum ActivityType
    {
        /// <summary>
        /// Game ("Playing Game/App")
        /// </summary>
        Game = 0,

        /// <summary>
        /// Streaming ("Streaming Game/App")
        /// </summary>
        Streaming = 1,

        /// <summary>
        /// Listening ("Listening to Song/Album")
        /// </summary>
        Listening = 2,

        /// <summary>
        /// Watching ("Watching Movie/Show")
        /// </summary>
        Watching = 3
    }
}
