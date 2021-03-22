// <copyright file="ReferencePlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.XPath;
using JuvoProcess;
using JuvoProcess.Bots;

/// <summary>
/// Plugin that provides reference/definition services.
/// </summary>
public class ReferencePlugin : IBotPlugin, IDisposable
{
    private const string BingUri  = "https://www.bing.com/search?q=define%3A+";
    private const string PathToDef = "/html/body/div[2]/main/ol/li[1]/div[6]/div[1]/div[3]/div/div/span/ol/li/div/div/div[1]";

    private readonly IBrowsingContext context;
    private          bool             disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencePlugin"/> class.
    /// </summary>
    public ReferencePlugin()
    {
        this.context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithXPath());
    }

    /// <inheritdoc/>
    public IList<string> Commands => new[] { "define" };

    /// <inheritdoc/>
    public IList<int>? CommandTimeMin => default;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient client)
    {
        var term = string.Join(' ', cmd.RequestText.Split(' ').Skip(1).ToArray());
        var document = await this.context.OpenAsync($"{BingUri}{term}");
        var cell = document.Body.SelectSingleNode(PathToDef);
        cmd.ResponseText = $"{term}: {cell.TextContent}";
        return cmd;
    }

    /// <summary>
    /// Disposes of any managed or unmanaged objects.
    /// </summary>
    /// <param name="disposing">Specifies whether the object is disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.context?.Dispose();
            }

            this.disposedValue = true;
        }
    }
}
