// <copyright file="ILog.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    /// <summary>
    /// /// Proxy ILog for implementing IJuvoLog.
    /// </summary>
    public interface ILog : log4net.ILog
    {
    }
}
