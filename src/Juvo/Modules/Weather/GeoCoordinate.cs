// <copyright file="GeoCoordinate.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    using System;

    /// <summary>
    /// Geographic location of a point.
    /// </summary>
    public struct GeoCoordinate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinate"/> struct.
        /// </summary>
        /// <param name="stringCoords">String containing coordinates in 'lat,long' format.</param>
        public GeoCoordinate(string stringCoords)
        {
            if (string.IsNullOrEmpty(stringCoords))
            {
                throw new ArgumentNullException(nameof(stringCoords));
            }

            var coordParts = stringCoords.Split(',');
            var latString = coordParts[0].Replace("E", string.Empty).Replace("°", string.Empty).Trim();
            var lonString = coordParts[0].Replace("N", string.Empty).Replace("°", string.Empty).Trim();

            if (coordParts.Length != 2 ||
                !double.TryParse(coordParts[0].Trim(), out double lat) ||
                !double.TryParse(coordParts[1].Trim(), out double lon))
            {
                throw new ArgumentOutOfRangeException(nameof(stringCoords));
            }

            this.Latitude = lat;
            this.Longitude = lon;
        }

        /// <summary>
        /// Gets or sets the latitude of the GeoCoordinate.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the GeoCoordinate.
        /// </summary>
        public double Longitude { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.Latitude},{this.Longitude}";
        }
    }
}
