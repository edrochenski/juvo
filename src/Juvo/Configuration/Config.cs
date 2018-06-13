// <copyright file="Config.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    /// <summary>
    /// Config.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the Discord configuration.
        /// </summary>
        public DiscordConfig Discord { get; set; }

        /// <summary>
        /// Gets or sets the IRC configuration.
        /// </summary>
        public IrcConfig Irc { get; set; }

        /// <summary>
        /// Gets or sets the Juvo configuration.
        /// </summary>
        public JuvoConfig Juvo { get; set; }

        /// <summary>
        /// Gets or sets the Slack configuration.
        /// </summary>
        public SlackConfig Slack { get; set; }

        /// <summary>
        /// Gets or sets the Stride configuration.
        /// </summary>
        public StrideConfig Stride { get; set; }

        /// <summary>
        /// Gets or sets the WebServer configuration.
        /// </summary>
        public WebServerConfig WebServer { get; set; }
    }
}