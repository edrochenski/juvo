// <copyright file="DarkSkyDataBlock.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules.Weather
{
    /// <summary>
    /// Represents the various weather phenomena occurring over a period of time.
    /// </summary>
    public class DarkSkyDataBlock
    {
        /// <summary>
        /// Gets or sets the data, an array of data points ordered by time.
        /// </summary>
        public DarkSkyDataPoint[] Data { get; set; }

        /// <summary>
        /// Gets or sets the icon, a machine-readable text summary of this data block.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the human-readable summary of this data block.
        /// </summary>
        public string Summary { get; set; }
    }
}
