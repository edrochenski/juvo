// <copyright file="DarkSkyResponse.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    using System;

    /// <summary>
    /// UTF-8 encoded response received from the DarkSky api.
    /// </summary>
    public class DarkSkyResponse
    {
        /// <summary>
        /// Gets or sets the alerts pertinent to the requested location.
        /// </summary>
        public DarkSkyAlert[] Alerts { get; set; }

        /// <summary>
        /// Gets or sets the data point containing the current weather
        /// conditions at the requested location.
        /// </summary>
        public DarkSkyDataPoint Currently { get; set; }

        /// <summary>
        /// Gets or sets the daily object, containing the weather conditions day-by-day
        /// for the next week.
        /// </summary>
        public DarkSkyDataBlock Daily { get; set; }

        /// <summary>
        /// Gets or sets the flags, which contains misc. metadata about the request.
        /// </summary>
        public DarkSkyFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the hourly object, containing the weather conditions hour-by-hour
        /// for the next two days.
        /// </summary>
        public DarkSkyDataBlock Hourly { get; set; }

        /// <summary>
        /// Gets or sets the requested latitude.
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Gets or sets the requested longitude.
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// Gets or sets the minutely object, containing the weather conditions minute-by-minute
        /// for the next hour.
        /// </summary>
        public DarkSkyDataBlock Minutely { get; set; }

        /// <summary>
        /// Gets or sets the IANA timezone for the requested location.
        /// </summary>
        /// <remarks>
        /// This is used for text summaries and for determining when hourly
        /// and daily block objects begin.
        /// </remarks>
        public string Timezone { get; set; }

        /// <summary>
        /// Gets or sets the current timezone offset in hours.
        /// </summary>
        [Obsolete("Use of this property will almost certainly result in Daylight Saving Time bugs. Please use timezone, instead.")]
        public decimal Offset { get; set; }
    }
}
