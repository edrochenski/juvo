// <copyright file="SlackUserProfile.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Slack
{
    /// <summary>
    /// Slack user profile.
    /// </summary>
    public struct SlackUserProfile
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the image (24).
        /// </summary>
        public string Image24 { get; set; }

        /// <summary>
        /// Gets or sets the image (32).
        /// </summary>
        public string Image32 { get; set; }

        /// <summary>
        /// Gets or sets the image (48).
        /// </summary>
        public string Image48 { get; set; }

        /// <summary>
        /// Gets or sets the image (72).
        /// </summary>
        public string Image72 { get; set; }

        /// <summary>
        /// Gets or sets the image (192).
        /// </summary>
        public string Image192 { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the real name.
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// Gets or sets the skype.
        /// </summary>
        public string Skype { get; set; }
    }
}
