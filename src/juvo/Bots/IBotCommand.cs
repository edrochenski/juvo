// <copyright file="IBotCommand.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    /// <summary>
    /// Represents a bot command mechanism.
    /// </summary>
    public interface IBotCommand
    {
        /// <summary>
        /// Gets or sets the bot associated with the command.
        /// </summary>
        IBot Bot { get; set; }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        string RequestText { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        string ResponseText { get; set; }
    }
}
