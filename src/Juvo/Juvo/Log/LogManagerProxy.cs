// <copyright file="LogManagerProxy.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using log4net;

    /// <summary>
    /// Proxy.
    /// </summary>
    public class LogManagerProxy : ILogManager
    {
        /// <inheritdoc/>
        public ILog GetLogger(Type type)
        {
            return new LogProxy(LogManager.GetLogger(type));
        }
    }
}
