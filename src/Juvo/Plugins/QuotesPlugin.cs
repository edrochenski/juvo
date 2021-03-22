// <copyright file="QuotesPlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.XPath;
using JuvoProcess;
using JuvoProcess.Bots;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// Plugin that provides reference/definition services.
/// </summary>
public class QuotesPlugin : IBotPlugin, IDisposable
{
    private const string XRoot         = "/html/body/div[1]/div/div/div[1]/div/div[2]/div/div/div[4]/div/div/div";
    private const string YahooQuoteUri = "https://finance.yahoo.com/quote/";

    private readonly IBrowsingContext context;
    private          bool             disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuotesPlugin"/> class.
    /// </summary>
    public QuotesPlugin()
    {
        this.context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithXPath());
    }

    /// <inheritdoc/>
    public IList<string> Commands => new[] { "q", "quote" };

    /// <inheritdoc/>
    public IList<int>? CommandTimeMin => default;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient juvo)
    {
        var cmdParts = cmd.RequestText.Split(' ');
        var symbol = cmdParts[1].ToUpperInvariant();

        try
        {
            using var document = await this.context.OpenAsync($"{YahooQuoteUri}{symbol}");

            if (document.StatusCode != HttpStatusCode.OK)
            {
                cmd.ResponseText = "Could not retrieve quote";
                return cmd;
            }

            var header   = document.Body.SelectSingleNode($"{XRoot}/div[2]/div[1]/div[1]/h1");
            var mkprice  = document.Body.SelectSingleNode($"{XRoot}/div[3]/div[1]/div/span[1]");
            var mkchange = document.Body.SelectSingleNode($"{XRoot}/div[3]/div[1]/div/span[2]");
            var mkasof   = document.Body.SelectSingleNode($"{XRoot}/div[3]/div[1]/div/div/span");
            var ahprice  = document.Body.SelectSingleNode($"{XRoot}/div[3]/div[1]/p/span[1]");
            var ahchange = document.Body.SelectSingleNode($"{XRoot}/div[3]/div[1]/p/span[2]/span");
            var ahasof   = document.Body.SelectSingleNode($"{XRoot}/div[3]/div[1]/p/span[3]");

            cmd.ResponseText = $"{header.TextContent}: {mkprice.TextContent}  {mkchange.TextContent} [{mkasof.TextContent}]";

            if (ahprice is INode)
            {
                cmd.ResponseText += $" After/Pre: {ahprice.TextContent}  {ahchange.TextContent} [{ahasof.TextContent}]";
            }
        }
        catch (JsonReaderException jsonException)
        {
            juvo.Log(LogLevel.Error, "Error while parsing/reading json", jsonException);
            cmd.ResponseText = "An error occurred while parsing the response.";
        }

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
