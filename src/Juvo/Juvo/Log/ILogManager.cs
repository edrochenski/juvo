// <copyright file="ILogManager.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;

    /// <summary>
    /// Represents a log manager.
    /// </summary>
    public interface ILogManager
    {
        /// <summary>
        /// Gets a logger.
        /// </summary>
        /// <param name="type">Type being logged.</param>
        /// <returns>A logger for the type.</returns>
        ILog GetLogger(Type type);
    }
}