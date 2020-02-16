// <copyright file="JuvoUser.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Juvo
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a user.
    /// </summary>
    public class JuvoUser
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's identities.
        /// </summary>
        public IEnumerable<IJuvoUserIdentity>? Identities { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}
