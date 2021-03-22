// <copyright file="HackerNewsPlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JuvoProcess;
using JuvoProcess.Bots;
using Newtonsoft.Json;

/// <summary>
/// Plugin that interacts with HackerNews.
/// </summary>
public class HackerNewsPlugin : IBotPlugin
{
    private const string UrlItem       = "https://hacker-news.firebaseio.com/v0/item/{0}.json";
    private const string UrlItemWeb    = "https://news.ycombinator.com/item?id={0}";
    private const string UrlMaxItemId  = "https://hacker-news.firebaseio.com/v0/maxitem.json";
    private const string UrlNewStories = "https://hacker-news.firebaseio.com/v0/newstories.json";

    private readonly IList<BotSource> subscribers = new List<BotSource>();
    private readonly IList<int> timeMin = new[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55 };

    private int maxItemId = -1;

    /// <inheritdoc/>
    public IList<string> Commands => new[] { "hn" };

    /// <inheritdoc/>
    public IList<int> CommandTimeMin => this.timeMin;

    /// <inheritdoc/>
    public async Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient juvo)
    {
        switch (cmd.TriggeredBy)
        {
            case CommandTriggerType.Timer:
            {
                if (this.maxItemId == -1)
                {
                    this.maxItemId = await this.GetMaxItemId(juvo);
                    return cmd;
                }
                else if (this.subscribers.Count > 0)
                {
                    await this.GetNewStories(juvo);
                }

                break;
            }

            case CommandTriggerType.User:
            {
                if (cmd.Bot is null) { return cmd; }

                switch (cmd.RequestText.ToLowerInvariant())
                {
                    case "hn":
                    {
                        var bs = new BotSource(cmd.Bot, cmd.Source);
                        if (this.subscribers.Contains(bs))
                        {
                            this.subscribers.Remove(bs);
                            cmd.ResponseText = $"Removed {cmd.Source.Identifier} from HackerNews subscription.";
                        }
                        else
                        {
                            this.subscribers.Add(bs);
                            cmd.ResponseText = $"Added {cmd.Source.Identifier} to HackerNews subscription.";
                        }

                        break;
                    }
                }

                break;
            }
        }

        return cmd;
    }

    private async Task<int> GetMaxItemId(IJuvoClient juvo)
    {
        using var response = await juvo.HttpClient.GetAsync(UrlMaxItemId);
        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<int>(await response.Content.ReadAsStringAsync());
        }

        return -1;
    }

    private async Task GetNewStories(IJuvoClient juvo)
    {
        try
        {
            using var newResponse = await juvo.HttpClient.GetAsync(UrlNewStories);
            if (!newResponse.IsSuccessStatusCode) { return; }

            var content = await newResponse.Content.ReadAsStringAsync();
            var storyList = JsonConvert.DeserializeObject<List<int>>(content);
            var storySent = 0;

            foreach (var storyId in storyList.OrderBy(x => x))
            {
                if (storyId <= this.maxItemId) { continue; }

                using var itemResponse = await juvo.HttpClient.GetAsync(string.Format(UrlItem, storyId));
                if (itemResponse.IsSuccessStatusCode)
                {
                    var hnItem = JsonConvert.DeserializeObject<HnItem>(await itemResponse.Content.ReadAsStringAsync());
                    if (hnItem is HnItem && !hnItem.Dead && !hnItem.Deleted)
                    {
                        foreach (var sub in this.subscribers)
                        {
                            if (storySent > 0) { await Task.Delay(5000); } // TODO: make configurable

                            var url = string.Format(UrlItemWeb, storyId);
                            var resp = new BotCommand(sub.Bot, CommandTriggerType.Timer, sub.Source, string.Empty)
                            { ResponseText = $"[{hnItem.By}] {hnItem.Title} @ {url}" };
                            await juvo.QueueResponse(resp);

                            storySent++;
                        }
                    }
                }

                this.maxItemId = storyId;
            }
        }
        catch (Exception exc)
        {
            juvo?.LogError("Exception occurred getting new stories", exc);
        }
    }
}

/// <summary>
/// Bot and source.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Simplification")]
public struct BotSource
{
    /// <summary>
    /// Gets or sets the bot object.
    /// </summary>
    public IBot Bot { get; set; }

    /// <summary>
    /// Gets or sets the source object.
    /// </summary>
    public CommandSource Source { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BotSource"/> struct.
    /// </summary>
    /// <param name="bot">Bot object.</param>
    /// <param name="source">Source object.</param>
    public BotSource(IBot bot, CommandSource source)
    {
        this.Bot = bot;
        this.Source = source;
    }

    /// <summary>
    /// Determines if two CommandSource objects are equal.
    /// </summary>
    /// <param name="left">Left-side object.</param>
    /// <param name="right">Right-side object.</param>
    /// <returns><code>true</code> if both objects are equal.</returns>
    public static bool operator ==(BotSource left, BotSource right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines if two CommandSource objects are equal.
    /// </summary>
    /// <param name="left">Left-side object.</param>
    /// <param name="right">Right-side object.</param>
    /// <returns><code>true</code> if both objects are equal.</returns>
    public static bool operator !=(BotSource left, BotSource right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is BotSource))
        {
            return false;
        }

        var temp = (BotSource)obj;
        return this.Bot == temp.Bot
            && this.Source == temp.Source;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = (int)2166136261;
            hash = (hash * 16777619) ^ this.Bot.GetHashCode();
            hash = (hash * 16777619) ^ this.Source.GetHashCode();

            return hash;
        }
    }
}

/// <summary>
/// HackerNews item.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Simplification")]
public class HnItem
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string By { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether or not the item is dead.
    /// </summary>
    public bool Dead { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the item has been deleted.
    /// </summary>
    public bool Deleted { get; set; }

    /// <summary>
    /// Gets or sets the total comment count for stories/polls.
    /// </summary>
    public int Descendents { get; set; }

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the kids (comments) in ranked order.
    /// </summary>
    public List<int> Kids { get; set; } = new List<int>();

    /// <summary>
    /// Gets or sets the parent Id.
    /// </summary>
    public int Parent { get; set; }

    /// <summary>
    /// Gets or sets the parts/pollopts, in display order.
    /// </summary>
    public List<int> Parts { get; set; } = new List<int>();

    /// <summary>
    /// Gets or sets the associated poll Id.
    /// </summary>
    public int Poll { get; set; }

    /// <summary>
    /// Gets or sets the associated poll Id.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time.
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the url.
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
