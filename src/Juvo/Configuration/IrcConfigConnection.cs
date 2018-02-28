// <copyright file="IrcConfigConnection.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// IRC configuration connection.
    /// </summary>
    public class IrcConfigConnection
    {
        /// <summary>
        /// Gets or sets the channels.
        /// </summary>
        public IEnumerable<IrcConfigChannel> Channels { get; set; }

        /// <summary>
        /// Gets or sets the command token.
        /// </summary>
        public string CommandToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connection is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the ident.
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bot should wait until its host is masked
        /// to join channels.
        /// </summary>
        public bool JoinOnHostMasked { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the network
        /// </summary>
        public string Network { get; set; }

        /// <summary>
        /// Gets or sets the password/token used when connecting to the network.
        /// </summary>
        public string NetworkToken { get; set; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the alternative nickname.
        /// </summary>
        public string NicknameAlt { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Pass { get; set; }

        /// <summary>
        /// Gets or sets the real name.
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// Gets or sets the servers.
        /// </summary>
        public IEnumerable<IrcConfigServer> Servers { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the user mode.
        /// </summary>
        public string UserMode { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; }
    }
}
