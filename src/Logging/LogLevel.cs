using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace juvo.Logging
{
    /// <summary>
    /// Determines what level or severity a logged event is. Typically also used
    /// as a setting to determine which LogEvents should be shown or logged elsewhere
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// A LogEvent would never use this, but this could be used as a setting
        /// to turn logging completely off
        /// </summary>
        Off = 0,
        /// <summary>
        /// Designates a severe error or exception that will most likely cause the
        /// application to abort
        /// </summary>
        Fatal = 10,
        /// <summary>
        /// Designates an error that may have been caught/handled so that
        /// the application could continue running.
        /// </summary>
        Error = 20,
        /// <summary>
        /// Designates a harmful situation that could possible lead to lower level
        /// (<value>Error</value> or <value>Fatal</value> events
        /// </summary>
        Warn = 30,
        /// <summary>
        /// Designates informational messages
        /// </summary>
        Info = 40,
        /// <summary>
        /// Designates fine-grained informational events helpful to debug the application.
        /// </summary>
        Debug = 50,
        /// <summary>
        /// Designates the most detailed possible LogEvents
        /// </summary>
        Trace = 60,
        /// <summary>
        /// Designates the default logging level
        /// </summary>
        Default = Info
    }
}
