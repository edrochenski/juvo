// <copyright file="PluginExample.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using JuvoProcess;
using JuvoProcess.Bots;

/// <summary>
/// Plugin example.
/// </summary>
public class PluginExample : IBotPlugin
{
    /// <summary>
    /// Gets commands associated with the plugin.
    /// </summary>
    public IList<string> Commands => new[] { "example" };

    /// <summary>
    /// Gets time points (in minutes) the plugin should be triggered.
    /// </summary>
    public IList<int>? CommandTimeMin => default;

    /// <summary>
    /// Methods called when the plugin is activated (by user, timer, etc.)
    /// </summary>
    /// <param name="cmd">Command details.</param>
    /// <param name="client">Client that triggered the command.</param>
    /// <returns>Command with response.</returns>
    public Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient client)
    {
        throw new System.NotImplementedException();
    }
}
