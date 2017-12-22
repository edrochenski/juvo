// <copyright file="GeoCoordinate.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    /// <summary>
    /// Geographic location of a point.
    /// </summary>
    public struct GeoCoordinate
    {
        /// <summary>
        /// Gets or sets the latitude of the GeoCoordinate.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the GeoCoordinate.
        /// </summary>
        public double Longitude { get; set; }
    }
}
