// <copyright file="CommandSource.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Bots
{
    /// <summary>
    /// Source of a bot command.
    /// </summary>
    public class CommandSource
    {
        /// <summary>
        /// Gets or sets the identifier (if any) assigned to the source.
        /// This will either be the channel or user name.
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        public CommandSourceType SourceType { get; set; }

        /// <summary>
        /// Determines if two CommandSource objects are equal.
        /// </summary>
        /// <param name="left">Left-side object.</param>
        /// <param name="right">Right-side object.</param>
        /// <returns><code>true</code> if both objects are equal.</returns>
        public static bool operator ==(CommandSource left, CommandSource right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two CommandSource objects are equal.
        /// </summary>
        /// <param name="left">Left-side object.</param>
        /// <param name="right">Right-side object.</param>
        /// <returns><code>true</code> if both objects are equal.</returns>
        public static bool operator !=(CommandSource left, CommandSource right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is CommandSource))
            {
                return false;
            }

            var temp = obj as CommandSource;
            return this.Identifier == temp?.Identifier
                && this.SourceType == temp?.SourceType;
        }

        /// <summary>
        /// Gets a value-based copy of the object.
        /// </summary>
        /// <returns>Copy of the this object.</returns>
        public CommandSource GetCopy()
        {
            return new CommandSource
            {
                Identifier = this.Identifier,
                SourceType = this.SourceType
            };
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)2166136261;
                hash = (hash * 16777619) ^ this.Identifier.GetHashCode();
                hash = (hash * 16777619) ^ this.SourceType.GetHashCode();

                return hash;
            }
        }
    }
}
