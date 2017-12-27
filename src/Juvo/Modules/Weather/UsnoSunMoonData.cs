// <copyright file="UsnoSunMoonData.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    /// <summary>
    /// Represents data retrieved from usno.navy.mil containing sun and moon data.
    /// </summary>
    public class UsnoSunMoonData
    {
        /// <summary>
        /// Gets or sets the API version.
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the information on the nearest primary phase to the date requested.
        /// </summary>
        public ClosestPhaseData ClosestPhase { get; set; }

        /// <summary>
        /// Gets or sets the current phase.
        /// </summary>
        public string CurPhase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested date was invild and
        /// the date changed (to today's date most likely.)
        /// </summary>
        public bool DateChanged { get; set; }

        /// <summary>
        /// Gets or sets the day.
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Gets or sets the day of the week.
        /// </summary>
        public string DayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error occurred.
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Gets or sets the fracillum.
        /// </summary>
        public string Fracillum { get; set; }

        /// <summary>
        /// Gets or sets the latitiude.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public double Lon { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Gets or sets the data for the moon.
        /// </summary>
        public DataPoint[] MoonData { get; set; }

        /// <summary>
        /// Gets or sets the data for the next moon.
        /// </summary>
        public DataPoint[] NextMoonData { get; set; }

        /// <summary>
        /// Gets or sets the data for the next sun.
        /// </summary>
        public DataPoint[] NextSunData { get; set; }

        /// <summary>
        /// Gets or sets the data for the sun.
        /// </summary>
        public DataPoint[] SunData { get; set; }

        /// <summary>
        /// Gets or sets the error type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the Year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Represents closest phase information.
        /// </summary>
        public class ClosestPhaseData
        {
            /// <summary>
            /// Gets or sets the date of the primary phase.
            /// </summary>
            public string Date { get; set; }

            /// <summary>
            /// Gets or sets the phase of the moon at the nearest primary phase to the
            /// date requested.
            /// </summary>
            public string Phase { get; set; }

            /// <summary>
            /// Gets or sets the time of the primary phase.
            /// </summary>
            public string Time { get; set; }
        }

        /// <summary>
        /// Represents a data point.
        /// </summary>
        public class DataPoint
        {
            /// <summary>
            /// Gets or sets the phenomena code.
            /// </summary>
            /// <remarks>
            /// BC = Begin civil twilight
            /// R  = Rise
            /// U  = Upper Transit
            /// S  = Set
            /// EC = End civil twilight
            /// L  = Lower Transit(above the horizon)
            /// ** = object continuously above the horizon
            /// -- = object continuously below the horizon
            /// ^^ = object continuously above the twilight limit
            /// ~~ = object continuously below the twilight limit
            /// </remarks>
            public string Phen { get; set; }

            /// <summary>
            /// Gets or sets the time of the phase.
            /// </summary>
            public string Time { get; set; }
        }
    }
}
