// <copyright file="ExamplePlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using JuvoProcess;
using JuvoProcess.Bots;

/// <summary>
/// Plugin example.
/// </summary>
public class ExamplePlugin : IBotPlugin
{
    /// <inheritdoc/>
    public IList<string> Commands => new[] { "example" };

    /// <inheritdoc/>
    public IList<int>? CommandTimeMin => default;

    /// <inheritdoc/>
    public Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient client)
    {
        cmd.ResponseText = "Hello from example plugin!";
        return Task.FromResult(cmd);
    }
}
