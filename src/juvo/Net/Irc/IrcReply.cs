// <copyright file="IrcReply.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;
    using System.Linq;

    /// <summary>
    /// IRC reply.
    /// </summary>
    public class IrcReply
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcReply"/> class.
        /// </summary>
        /// <param name="rawMessage">Raw message.</param>
        public IrcReply(string rawMessage)
        {
            // ignore sects[0] since it should just be empty
            string[] sects = rawMessage.Split(new char[] { ':' }, 3);

            if (sects.Length > 1)
            {
                string[] parts = sects[1].Split(' ');

                if (parts.Length > 0)
                {
                    this.Prefix = parts[0];
                }

                if (parts.Length > 1)
                {
                    this.Command = parts[1];
                }

                if (parts.Length > 2)
                {
                    this.Target = parts[2];
                }

                if (parts.Length > 3)
                {
                    this.Params = new string[parts.Length - 3];
                    for (int x = 3; x < parts.Length; ++x)
                    {
                        this.Params[x - 3] = parts[x];
                    }
                }
            }

            if (sects.Length > 2)
            {
                this.Trailing = sects[2];
            }
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public string Command { get; protected set; }

        /// <summary>
        /// Gets or sets the params.
        /// </summary>
        public string[] Params { get; protected set; }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        public string Prefix { get; protected set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public string Target { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the target is a channel.
        /// </summary>
        public bool TargetIsChannel
            => !string.IsNullOrEmpty(this.Target) && IrcClient.ChannelIdents.Contains(this.Target[0]);

        /// <summary>
        /// Gets or sets a value indicating whether the target is a channel.
        /// </summary>
        public bool TargetIsUser { get; protected set; }

        /// <summary>
        /// Gets or sets trailing text.
        /// </summary>
        public string Trailing { get; protected set; }
    }
}
