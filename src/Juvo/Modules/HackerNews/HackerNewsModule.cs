// <copyright file="HackerNewsModule.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>
namespace JuvoProcess.Modules.HackerNews
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Net;
    using Newtonsoft.Json;

    /// <summary>
    /// Hacker News module.
    /// </summary>
    /// <remarks>
    /// Largely based on the API @ https://github.com/HackerNews/API.
    /// </remarks>
    public class HackerNewsModule : IBotModule
    {
    /*/ Constants /*/
        private const string ItemUrl = "https://hacker-news.firebaseio.com/v0/item/";
        private const string ModuleName = "hackernews";
        private const string NewStoryUrl = "https://hacker-news.firebaseio.com/v0/newstories.json";

    /*/ Fields /*/
        private readonly Dictionary<IBot, List<CommandSource>> bots;
        private readonly IJuvoClient client;
        private readonly IHttpClient httpClient;
        private readonly Timer timer;

        private int lastItemId = 0;

    /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="HackerNewsModule"/> class.
        /// </summary>
        /// <param name="client">Juvo client.</param>
        /// <param name="httpClient">Http client.</param>
        public HackerNewsModule(IJuvoClient client, IHttpClient httpClient)
        {
            this.client = client;
            this.httpClient = httpClient;

            this.bots = new Dictionary<IBot, List<CommandSource>>();
            this.timer = new Timer(this.CheckNew, null, 0, 15000);
        }

    /*/ Methods /*/

        /// <inheritdoc />
        public async Task Execute(IBotCommand cmd)
        {
            var cmdTokens = cmd.RequestText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (cmdTokens[0].ToLowerInvariant())
            {
                case "hn":
                    if (this.bots.ContainsKey(cmd.Bot))
                    {
                        if (this.bots[cmd.Bot].Contains(cmd.Source))
                        {
                            this.bots[cmd.Bot].Remove(cmd.Source);
                            cmd.ResponseText =
                                $"Removed '{cmd.Source.Identifier}' from HackerNews notifications";
                        }
                        else
                        {
                            this.bots[cmd.Bot].Add(cmd.Source);
                            cmd.ResponseText =
                                $"Added '{cmd.Source.Identifier}' to HackerNews notifications";
                        }
                    }
                    else
                    {
                        this.bots.Add(cmd.Bot, new List<CommandSource> { cmd.Source });
                        cmd.ResponseText =
                            $"Added '{cmd.Source.Identifier}' to HackerNews notifications";
                    }

                    break;
            }

            await Task.CompletedTask;
        }

        private void CheckNew(object sender)
        {
            try
            {
                using (var resp = this.httpClient.GetAsync(NewStoryUrl).Result)
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        var itemIDs = JsonConvert.DeserializeObject<int[]>(resp.Content.ReadAsStringAsync().Result);
                        if (itemIDs != null && itemIDs.Length > 0)
                        {
                            if (this.lastItemId == 0)
                            {
                                this.lastItemId = itemIDs[0];
                                this.client.Log.Info($"[{ModuleName}] Set initial last item to {this.lastItemId}");
                            }

                            for (var i = itemIDs.Length - 1; i >= 0; --i)
                            {
                                if (itemIDs[i] > this.lastItemId)
                                {
                                    this.lastItemId = itemIDs[i];
                                    this.client.Log.Debug($"[{ModuleName}] New item: {itemIDs[i]}");

                                    if (this.HasListeners())
                                    {
                                        try
                                        {
                                            using (var itemResponse = this.httpClient.GetAsync($"{ItemUrl}{itemIDs[i]}.json").Result)
                                            {
                                                if (resp.StatusCode == HttpStatusCode.OK)
                                                {
                                                    var item = JsonConvert.DeserializeObject<HackerNewsItem>(itemResponse.Content.ReadAsStringAsync().Result);
                                                    var response = $"{item.By}: {item.Title} ({item.Url})";

                                                    foreach (var x in this.bots)
                                                    {
                                                        foreach (var y in x.Value)
                                                        {
                                                            x.Key.QueueResponse(new BotCommand
                                                            {
                                                                Bot = x.Key,
                                                                ResponseText = response,
                                                                Source = y
                                                            });
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    this.client.Log.Warn(
                                                        $"[{ModuleName}] Received {resp.StatusCode} when retreving item {itemIDs[i]}");
                                                }
                                            }
                                        }
                                        catch (Exception exc)
                                        {
                                            this.client.Log.Error($"[{ModuleName}] {exc.Message}", exc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                this.client.Log.Error($"[{ModuleName}] {exc.Message}", exc);
            }
        }

        private bool HasListeners()
        {
            foreach (var x in this.bots)
            {
                if (x.Value.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
