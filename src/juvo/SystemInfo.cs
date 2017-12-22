// <copyright file="SystemInfo.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System.IO;

    /// <summary>
    /// System information.
    /// </summary>
    public struct SystemInfo
    {
        /// <summary>
        /// Gets or sets the application's full roaming data path.
        /// </summary>
        public DirectoryInfo AppDataPath { get; set; }

        /// <summary>
        /// Gets or sets the OS the application is running on.
        /// </summary>
        public OperatingSystem Os { get; set; }

        /// <summary>
        /// Gets or sets the application's full local data path.
        /// </summary>
        public DirectoryInfo LocalAppDataPath { get; set; }
    }
}
