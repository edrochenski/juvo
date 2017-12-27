// <copyright file="GeoCoordinate.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    using System;
    using System.Text.RegularExpressions;

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
            var latString = Regex.Replace(coordParts[0], "[^0-9-.]", string.Empty);
            var lonString = Regex.Replace(coordParts[1], "[^0-9-.]", string.Empty);

            if (coordParts.Length != 2 ||
                !double.TryParse(latString, out double lat) ||
                !double.TryParse(lonString, out double lon))
            {
                throw new ArgumentOutOfRangeException(nameof(stringCoords));
            }

            this.Latitude = lat;
            this.Longitude = lon;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoCoordinate"/> struct.
        /// </summary>
        /// <param name="lat">Latitude of coordinates.</param>
        /// <param name="lon">Longitude of coordinates.</param>
        public GeoCoordinate(double lat, double lon)
        {
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
