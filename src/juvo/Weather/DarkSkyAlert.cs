// <copyright file="DarkSkyAlert.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Weather
{
    /// <summary>
    /// Represents a severe weather report for an area.
    /// </summary>
    public class DarkSkyAlert
    {
        /// <summary>
        /// Gets or sets the detailed description of the alert.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the time that the alert expires.
        /// </summary>
        public long Expires { get; set; }

        /// <summary>
        /// Gets or sets the regions covered by the alert.
        /// </summary>
        public string[] Regions { get; set; }

        /// <summary>
        /// Gets or sets the severity of the alert.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Gets or sets the time the alert was issued.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the title, a brief description.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the URI for detailed info about the alert.
        /// </summary>
        public string Uri { get; set; }
    }
}
