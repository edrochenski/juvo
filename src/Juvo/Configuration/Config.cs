// <copyright file="Config.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

/* Hierarchy of config file
 * ========================
 *
 *  Configuration File (root)
 *      |
 *      |-- IrcConfig ("irc")
 *      |       |
 *      |       |-- Fields...
 *      |       |
 *      |       |-- IrcConfigServer[] ("servers")
 *      |       |     `-- Fields...
 *      |       |
 *      |       `-- IrcConfigChannel[] ("channels")
 *      |             `-- Fields...
 *      |
 *
 */

namespace JuvoProcess.Configuration
{
    /// <summary>
    /// Config.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the Discord config.
        /// </summary>
        public DiscordConfig Discord { get; set; }

        /// <summary>
        /// Gets or sets the IRC config.
        /// </summary>
        public IrcConfig Irc { get; set; }

        /// <summary>
        /// Gets or sets the Slack config.
        /// </summary>
        public SlackConfig Slack { get; set; }
    }
}