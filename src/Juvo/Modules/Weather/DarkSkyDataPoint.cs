// <copyright file="DarkSkyDataPoint.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    /// <summary>
    /// A collection of properties each representing the average (unless
    /// otherwise specified) of a particular weather phenomenon occuring
    /// during a period of time.
    /// </summary>
    public class DarkSkyDataPoint
    {
        /// <summary>
        /// Gets or sets the apparent ("feels like") temperature. (-daily)
        /// </summary>
        public decimal ApparentTemperature { get; set; }

        /// <summary>
        /// Gets or sets the max apparent ("feels like") temperature. (+daily)
        /// </summary>
        public decimal ApparentTemperatureMax { get; set; }

        /// <summary>
        /// Gets or sets the time of the max apparent ("feels like") temperature. (+daily)
        /// </summary>
        public long ApparentTemperatureMaxTime { get; set; }

        /// <summary>
        /// Gets or sets the min apparent ("feels like") temperature. (+daily)
        /// </summary>
        public decimal ApparentTemperatureMin { get; set; }

        /// <summary>
        /// Gets or sets the time of the min apparent ("feels like") temperature. (+daily)
        /// </summary>
        public long ApparentTemperatureMinTime { get; set; }

        /// <summary>
        /// Gets or sets the cloud cover, the % of the sky occluded by clouds (0 to 1, inclusive).
        /// </summary>
        public decimal CloudCover { get; set; }

        /// <summary>
        /// Gets or sets the dew point.
        /// </summary>
        public decimal DewPoint { get; set; }

        /// <summary>
        /// Gets or sets the relative humidity (0 to 1, inclusive).
        /// </summary>
        public decimal Humidity { get; set; }

        /// <summary>
        /// Gets or sets the icon, a machine-readable text summary of this
        /// current data point.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the moon phase, the fractional part of the lunation number during
        /// any given day. (+daily)
        /// </summary>
        public decimal MoonPhase { get; set; }

        /// <summary>
        /// Gets or sets the nearest storm bearing, the approcimate direction of the nearest storm in
        /// degrees, with true north at 0 and progressing clockwise. (+currently)
        /// </summary>
        public decimal NearestStormBearing { get; set; }

        /// <summary>
        /// Gets or sets the nearest storm distance, the approximate distance to the nearest storm in
        /// miles. (+currently)
        /// </summary>
        public decimal NearestStormDistance { get; set; }

        /// <summary>
        /// Gets or sets the ozone, the columnar density of total atmospheric ozone at the given time
        /// in Dobson units.
        /// </summary>
        public decimal Ozone { get; set; }

        /// <summary>
        /// Gets or sets the precip accumulation, the amount of snowfall accumulation expected to occur. (+hourly,daily)
        /// </summary>
        public decimal PrecipAccumulation { get; set; }

        /// <summary>
        /// Gets or sets the precip intensity, measured in inches of liquid water per hour. This value is conditional on
        /// probability for minutely, and unconditional otherwise.
        /// </summary>
        public decimal PrecipIntensity { get; set; }

        /// <summary>
        /// Gets or sets the precip intensity maximum. (+daily)
        /// </summary>
        public decimal PrecipIntensityMax { get; set; }

        /// <summary>
        /// Gets or sets the precip intensity maximum time. (+daily)
        /// </summary>
        public long PrecipIntensityMaxTime { get; set; }

        /// <summary>
        /// Gets or sets the precip probability, between 0 and 1 inclusive.
        /// </summary>
        public decimal PrecipProbability { get; set; }

        /// <summary>
        /// Gets or sets the type of the precip, defined if <c>PrecipIntensity</c> > 0.
        /// </summary>
        public string PrecipType { get; set; }

        /// <summary>
        /// Gets or sets the pressure, the sea-level air pressure in mullibars.
        /// </summary>
        public decimal Pressure { get; set; }

        /// <summary>
        /// Gets or sets the summary, a human-readable text summary of this data point.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the sunrise time. (+daily)
        /// </summary>
        public long SunriseTime { get; set; }

        /// <summary>
        /// Gets or sets the sunset time. (+daily)
        /// </summary>
        public long SunsetTime { get; set; }

        /// <summary>
        /// Gets or sets the temperature, the air temp in F. (-minutely)
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Gets or sets the temperature maximum. (+daily)
        /// </summary>
        public decimal TemperatureMax { get; set; }

        /// <summary>
        /// Gets or sets the temperature maximum time. (+daily)
        /// </summary>
        public long TemperatureMaxTime { get; set; }

        /// <summary>
        /// Gets or sets the temperature minimum. (+daily)
        /// </summary>
        public decimal TemperatureMin { get; set; }

        /// <summary>
        /// Gets or sets the temperature minimum time. (+daily)
        /// </summary>
        public long TemperatureMinTime { get; set; }

        /// <summary>
        /// Gets or sets the time at which this data point begins. Times are always aligned to the
        /// top of their respective unit (ex. minutely to the minute), according to the local timezone.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the index of the uv.
        /// </summary>
        public decimal UvIndex { get; set; }

        /// <summary>
        /// Gets or sets the uv index time, when the max uvIndex occurs during the day. (+daily)
        /// </summary>
        public long UvIndexTime { get; set; }

        /// <summary>
        /// Gets or sets the average visibility in miles, capped at 10mi.
        /// </summary>
        public decimal Visibility { get; set; }

        /// <summary>
        /// Gets or sets the wind bearing, the direction the wind is coming from in degrees.
        /// </summary>
        public decimal WindBearing { get; set; }

        /// <summary>
        /// Gets or sets the wind gust speed, in mph.
        /// </summary>
        public decimal WindGust { get; set; }

        /// <summary>
        /// Gets or sets the wind gust time, when the maximum wind gusts will occur. (+daily)
        /// </summary>
        public long WindGustTime { get; set; }

        /// <summary>
        /// Gets or sets the wind speed, in mph.
        /// </summary>
        public decimal WindSpeed { get; set; }
    }
}
