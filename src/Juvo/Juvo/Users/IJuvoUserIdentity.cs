// <copyright file="IJuvoUserIdentity.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Juvo
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a user identity.
    /// </summary>
    public interface IJuvoUserIdentity
    {
        /// <summary>
        /// Gets or sets masks for the identity.
        /// </summary>
        IEnumerable<string> Masks { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the user identity type.
        /// </summary>
        UserIdentityType Type { get; set; }

        /// <summary>
        /// Gets or sets the type ID.
        /// </summary>
        string TypeId { get; set; }
    }
}
