// <copyright file="JuvoUserIdentity.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Juvo
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a generic user identity.
    /// </summary>
    public class JuvoUserIdentity : IJuvoUserIdentity
    {
        /// <inheritdoc/>
        public IEnumerable<string>? Masks { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public UserIdentityType Type { get; set; } = UserIdentityType.None;

        /// <inheritdoc/>
        public string TypeId { get; set; } = string.Empty;
    }
}
