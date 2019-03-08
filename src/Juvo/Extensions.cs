// <copyright file="Extensions.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Container for extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Decodes a base-64 string.
        /// </summary>
        /// <param name="base64Text">Base-64 text.</param>
        /// <returns>A decoded string.</returns>
        public static string FromBase64(this string base64Text)
        {
            var base64bytes = Convert.FromBase64String(base64Text);
            return Encoding.UTF8.GetString(base64bytes);
        }

        /// <summary>
        /// Generates a salted hash value using <see cref="SHA256Managed"/> algorithm.
        /// </summary>
        /// <param name="value">Value to hash.</param>
        /// <param name="salt">Salt to hash.</param>
        /// <returns>Computed hash value.</returns>
        public static byte[] GenerateSaltedHash(this byte[] value, byte[] salt)
            => GenerateSaltedHash(value, salt, new SHA256Managed());

        /// <summary>
        /// Generates a salted hash value using the provided algorithm.
        /// </summary>
        /// <param name="value">Value to hash.</param>
        /// <param name="salt">Salt to hash.</param>
        /// <param name="algorithm">Hashing algorithm to use.</param>
        /// <returns>Computed hash value.</returns>
        public static byte[] GenerateSaltedHash(this byte[] value, byte[] salt, HashAlgorithm algorithm)
        {
            var textWithSalt = new byte[value.Length + salt.Length];

            for (var i = 0; i < value.Length; ++i)
            { textWithSalt[i] = value[i]; }

            for (var i = 0; i < salt.Length; ++i)
            {
                textWithSalt[value.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(textWithSalt);
        }

        /// <summary>
        /// Generates a salted hash value using <see cref="SHA256Managed"/> algorithm.
        /// </summary>
        /// <param name="value">Value to hash.</param>
        /// <param name="salt">Salt to hash.</param>
        /// <returns>Computed hash value.</returns>
        public static string GenerateSaltedHash(this string value, string salt)
            => GenerateSaltedHash(value, salt, new SHA256Managed());

        /// <summary>
        /// Generates a salted hash value using the provided algorithm.
        /// </summary>
        /// <param name="value">Value to hash.</param>
        /// <param name="salt">Salt to hash.</param>
        /// <param name="algorithm">Hashing algorithm to use.</param>
        /// <returns>Computed hash value.</returns>
        public static string GenerateSaltedHash(this string value, string salt, HashAlgorithm algorithm)
        {
            if (string.IsNullOrEmpty(value)) { throw new ArgumentNullException(nameof(value)); }
            if (string.IsNullOrEmpty(salt)) { throw new ArgumentNullException(nameof(salt)); }

            var result = GenerateSaltedHash(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(salt));
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Encodes a string into a base-64 representation.
        /// </summary>
        /// <param name="text">Text to encode.</param>
        /// <returns>Base-64 encoded string.</returns>
        public static string ToBase64(this string text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
