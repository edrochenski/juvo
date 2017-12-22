// <copyright file="WeatherModule.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    using JuvoProcess.Bots;

    /// <summary>
    /// A module for handling weather requests.
    /// </summary>
    public class WeatherModule : IBotModule
    {
/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherModule"/> class.
        /// </summary>
        public WeatherModule()
        {
        }

/*/ Methods /*/

        /// <summary>
        /// Attempts to find coordinates associated with a location.
        /// </summary>
        /// <param name="location">Location text.</param>
        /// <returns>If successful, a <see cref="GeoCoordinate"/> with valid lat/long values.</returns>
        public static GeoCoordinate? FindCoordinates(string location)
        {
            return null;
        }

        /// <inheritdoc />
        public void Execute(IBotCommand cmd)
        {
            cmd.ResponseText = "Weather!";
        }
    }
}
