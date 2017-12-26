// <copyright file="DarkSkyFlags.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    /// <summary>
    /// Represents the misc. metadata associated with a request.
    /// </summary>
    public class DarkSkyFlags
    {
        /// <summary>
        /// Gets or sets a value indicating whether there data for the location
        /// has been made unavailable.
        /// </summary>
        public bool DarkSkyUnavailable { get; set; }

        /// <summary>
        /// Gets or sets the isd stations.
        /// </summary>
        public string[] IsdStations { get; set; }

        /// <summary>
        /// Gets or sets the IDs of the sources utilized in servicing the request. See:
        /// https://darksky.net/dev/docs/sources
        /// </summary>
        public string[] Sources { get; set; }

        /// <summary>
        /// Gets or sets the units which were used for the data in this request.
        /// </summary>
        public string Units { get; set; }
    }
}
