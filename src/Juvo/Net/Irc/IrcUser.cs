// <copyright file="IrcUser.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    /// <summary>
    /// IRC user.
    /// </summary>
    public class IrcUser
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcUser"/> class.
        /// </summary>
        /// <param name="identifier">User identifier.</param>
        public IrcUser(string identifier)
        {
            if (identifier.Contains("!") && identifier.Contains("@"))
            {
                string[] parts = identifier.Split('@');
                string[] userParts = parts[0].Split('!');
                this.Host = parts[1];
                this.Nickname = userParts[0];
                this.Username = userParts[1];
            }
            else
            {
                this.Nickname = identifier;
            }
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        public string Host { get; protected set; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        public string Nickname { get; protected set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; protected set; }
    }
}
