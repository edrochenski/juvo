// <copyright file="ReferencePlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JuvoProcess;
using JuvoProcess.Bots;

/// <summary>
/// Plugin that provides reference/definition services.
/// </summary>
public class TestPlugin : IBotPlugin
{
    /// <inheritdoc/>
    public IList<string> Commands => new[] { "test" };

    /// <inheritdoc/>
    public IList<int>? CommandTimeMin => default;

    /// <inheritdoc/>
    public async Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient client)
    {
        cmd.ResponseText = $"Received '{cmd.RequestText}' @ {DateTime.Now}";
        return await Task.FromResult(cmd);
    }
}
