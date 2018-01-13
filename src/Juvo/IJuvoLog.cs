// <copyright file="IJuvoLog.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;

    /// <summary>
    /// Represents a Juvo process logger.
    /// </summary>
    public interface IJuvoLog
    {
        /// <summary>
        /// Writes a debug message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void Debug(object message);

        /// <summary>
        /// Writes a debug message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exc">Exception to include.</param>
        void Debug(object message, Exception exc);

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void Error(object message);

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exc">Exception to include.</param>
        void Error(object message, Exception exc);

        /// <summary>
        /// Writes a fatal message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void Fatal(object message);

        /// <summary>
        /// Writes a fatal message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exc">Exception to include.</param>
        void Fatal(object message, Exception exc);

        /// <summary>
        /// Writes an info message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void Info(object message);

        /// <summary>
        /// Writes an info message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exc">Exception to include.</param>
        void Info(object message, Exception exc);

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        void Warn(object message);

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="exc">Exception to include.</param>
        void Warn(object message, Exception exc);
    }
}
