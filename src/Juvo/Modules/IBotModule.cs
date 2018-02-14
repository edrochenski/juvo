// <copyright file="IBotModule.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Modules
{
    using System.Threading.Tasks;
    using JuvoProcess.Bots;

    /// <summary>
    /// Represents a bot module.
    /// </summary>
    public interface IBotModule
    {
        /// <summary>
        /// Executes the module.
        /// </summary>
        /// <param name="cmd">Originating command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        Task Execute(IBotCommand cmd);
    }
}
