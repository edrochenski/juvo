﻿// <copyright file="JuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net;
    using JuvoProcess.Net.Discord;
    using JuvoProcess.Resources;
    using JuvoProcess.Resources.Commands;
    using JuvoProcess.Resources.Logging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Juvo client.
    /// </summary>
    public class JuvoClient : IJuvoClient
    {
        /*/ Constants /*/

        private const int TimerTickRate = 10;

        /*/ Fields /*/

        private readonly Queue<IBotCommand> commandQueue;
        private readonly Dictionary<string[], Func<IBotCommand, Task>> commands;
        private readonly Timer commandTimer;
        private readonly IDiscordBotFactory discordBotFactory;
        private readonly List<IDiscordBot> discordBots;
        private readonly IIrcBotFactory ircBotFactory;
        private readonly List<IIrcBot> ircBots;
        private readonly ILog log;
        private readonly Dictionary<string[], IBotPlugin> plugins;
        private readonly ISlackBotFactory slackBotFactory;
        private readonly List<ISlackBot> slackBots;
        private readonly ManualResetEvent resetEvent;
        private readonly IWebHost webServer;
        private readonly CancellationToken webHostToken;

        private Config config;
        private string lastPerf;
        private DateTime lastPerfTime;
        private Mutex lastPerfLock;
        private DateTime started;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="JuvoClient"/> class.
        /// </summary>
        /// <param name="configuration">Bot's configuration.</param>
        /// <param name="discordBotFactory">Factory object for Discord bots.</param>
        /// <param name="ircBotFactory">Factory object for IRC bots.</param>
        /// <param name="slackBotFactory">Factory object for Slack bots.</param>
        /// <param name="webServer">Web server for managing the bot.</param>
        /// <param name="logManager">Log manager.</param>
        /// <param name="resetEvent">Manual reset object for thread.</param>
        public JuvoClient(
            Config configuration,
            IDiscordBotFactory discordBotFactory,
            IIrcBotFactory ircBotFactory,
            ISlackBotFactory slackBotFactory,
            ILogManager logManager,
            IWebHost webServer,
            ManualResetEvent resetEvent = null)
        {
            this.config = configuration ?? throw new ArgumentException(nameof(configuration));
            this.discordBotFactory = discordBotFactory ?? throw new ArgumentNullException(nameof(discordBotFactory));
            this.ircBotFactory = ircBotFactory ?? throw new ArgumentNullException(nameof(ircBotFactory));
            this.resetEvent = resetEvent;
            this.slackBotFactory = slackBotFactory ?? throw new ArgumentNullException(nameof(slackBotFactory));
            this.webHostToken = default(CancellationToken);
            this.webServer = webServer;

            this.commandQueue = new Queue<IBotCommand>();
            this.commandTimer = new Timer(this.CommandTimerTick, null, TimerTickRate, TimerTickRate);
            this.discordBots = new List<IDiscordBot>();
            this.ircBots = new List<IIrcBot>();
            this.lastPerfLock = new Mutex();
            this.log = logManager?.GetLogger(typeof(JuvoClient));
            this.plugins = new Dictionary<string[], IBotPlugin>();
            this.slackBots = new List<ISlackBot>();
            this.started = DateTime.UtcNow;
            this.State = JuvoState.Idle;

            // this block maps the bots internal commands to the appropriate
            // internal methods.
            // ?: should these commands be overridable in some way, for example
            // if a plugin/module wanted to handle one of these.
            this.commands = new Dictionary<string[], Func<IBotCommand, Task>>
            {
                { new[] { "shutdown", "die" }, this.CommandShutdown },
                { new[] { "perf", "performance" }, this.CommandPerf },
                { new[] { "ros", "roslyn" }, this.CommandRoslyn },
                { new[] { "set" }, this.CommandSet },
                { new[] { "status" }, this.CommandStatus }
            };

            this.LoadPlugins();
        }

        /*/ Properties /*/

        /// <inheritdoc/>
        public Config Config => this.config;

        /// <inheritdoc/>
        public ILog Log => this.log;

        /// <summary>
        /// Gets or sets the bot's current state.
        /// </summary>
        public JuvoState State { get; protected set; }

        /// <inheritdoc/>
        public SystemInfo SystemInfo => this.config.System;

        /*/ Methods /*/

        /// <summary>
        /// Queues command for the bot to execute or pass on.
        /// </summary>
        /// <param name="cmd">Command object.</param>
        public void QueueCommand(IBotCommand cmd)
        {
            Debug.Assert(cmd != null, "cmd == null");
            Debug.Assert(!string.IsNullOrEmpty(cmd.RequestText), "cmd.RequestText != null/empty");

            this.commandQueue.Enqueue(cmd);
            this.log?.Info(DebugResx.CommandEnqueued);
        }

        /// <summary>
        /// Starts the bot.
        /// </summary>
        /// <returns>Result of the call.</returns>
        public async Task Run()
        {
            this.log?.Info(InfoResx.CreatingMissingResources);
            this.CreateResources();

            this.log?.Info(InfoResx.LoadingConfigFile);
            this.LoadConfig();

            this.log?.Info(InfoResx.StartingBots);
            await this.StartBots();

            if (this.config.WebServer.Enabled)
            {
                this.log?.Info(InfoResx.StartingWebServer);
                await this.webServer.RunAsync(this.webHostToken);
            }

            this.State = JuvoState.Running;
            this.log?.Info(InfoResx.BotRunning);
        }

        /// <summary>
        /// Called when <see cref="commandTimer" /> tick occurs.
        /// </summary>
        /// <param name="state">State object.</param>
        protected void CommandTimerTick(object state)
        {
            if (this.commandQueue.Count > 0)
            {
                lock (this)
                {
                    IBotCommand cmd;
                    while (this.commandQueue.Count > 0 && (cmd = this.commandQueue.Dequeue()) != null)
                    {
                        this.ProcessCommand(cmd).Wait();
                    }
                }
            }

            lock (this.lastPerfLock)
            {
                if (DateTime.Now.Minute % 5 == 0 && DateTime.Now.Second == 0 &&
                    (this.lastPerf == null || this.lastPerfTime.AddMinutes(5) < DateTime.UtcNow))
                {
                    this.LogPerf();
                }
            }
        }

        /// <summary>
        /// Commands the bot to return the latest perf results.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandPerf(IBotCommand command)
        {
            var cmdTokens = command.RequestText.Split(' ');

            if (cmdTokens.Length == 1)
            {
                command.ResponseText = this.lastPerf ?? PerfResx.NoInfoCollected;
            }
            else
            {
                switch (cmdTokens[1].ToLowerInvariant())
                {
                    case "thread":
                        if (cmdTokens.Length > 2)
                        {
                            if (!int.TryParse(cmdTokens[2], out int threadNumber))
                            {
                                command.ResponseText =
                                    string.Format(PerfResx.ThreadNumNotRecognized, cmdTokens[2]);
                                return;
                            }

                            var procThreads = Process.GetCurrentProcess().Threads;
                            if (threadNumber > procThreads.Count)
                            {
                                command.ResponseText =
                                    string.Format(PerfResx.ThreadNumOutsideBounds, threadNumber);
                                return;
                            }

                            var thread = procThreads[threadNumber];
                            var waitReason = thread.ThreadState == System.Diagnostics.ThreadState.Wait
                                ? $" ({thread.WaitReason})"
                                : string.Empty;

                            command.ResponseText =
                                $"[{thread.Id}] {CommonResx.State}: {thread.ThreadState}{waitReason}, " +
                                $"{CommonResx.Priority}: {thread.BasePriority}/{thread.CurrentPriority} ({thread.PriorityLevel}), " +
                                $"{CommonResx.Started}: {thread.StartTime}, " +
                                $"TPT: {thread.TotalProcessorTime.TotalSeconds:0.00}s, " +
                                $"UPT: {thread.UserProcessorTime.TotalSeconds:0.00}s";
                        }

                        break;

                    case "threads":
                        var added = 0;
                        var output = new StringBuilder();
                        var threads = Process.GetCurrentProcess().Threads;

                        for (var i = 0; i < threads.Count; ++i)
                        {
                            if (threads[i].TotalProcessorTime.TotalSeconds > 0d)
                            {
                                continue;
                            }

                            if (added > 0)
                            {
                                output.Append(" ");
                            }

                            output.Append(
                                $"[{i}]{threads[i].Id}/" +
                                $"{threads[i].TotalProcessorTime.TotalSeconds:0.00}s");

                            if (threads[i].ThreadState != System.Diagnostics.ThreadState.Wait)
                            {
                                output.Append($" ({threads[i].ThreadState})");
                            }

                            added++;
                        }

                        command.ResponseText = output.ToString();

                        break;

                    default:
                        command.ResponseText =
                            string.Format(PerfResx.CommandNotRecognized, cmdTokens[1]);
                        break;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Commands the bot to compile code using roslyn.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandRoslyn(IBotCommand command)
        {
            try
            {
                var comp = CSharpCompilation.Create("JuvoRuntime")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).GetType().Assembly.Location))
                    .AddSyntaxTrees(CSharpSyntaxTree.ParseText(
                    #pragma warning disable SA1118 // Parameter must not span multiple lines
                    $"namespace Juvo " +
                    $"{{ " +
                    $"  using System;" +
                    $"  using System.Text;" +
                    $"  public class Program " +
                    $"  {{ " +
                    $"      public static string Main() " +
                    $"      {{ " +
                    $"          var outputBuilder = new StringBuilder();" +
                    $"          {string.Join(' ', command.RequestText.Replace("_out_", "outputBuilder.Append").Split(' ').Skip(1).ToArray())} " +
                    $"          return outputBuilder.ToString();" +
                    $" return string.Empty;" +
                    $"      }} " +
                    $"  }} " +
                    $"}}"));
#pragma warning restore SA1118 // Parameter must not span multiple lines

                using (var memory = new MemoryStream())
                {
                    var fileName = $"{DateTime.UtcNow.Ticks.ToString()}.dll";
                    var results = comp.Emit(memory);
                    if (results.Success)
                    {
                        await memory.FlushAsync();

                        var domain = AppDomain.CreateDomain("Plugin");
                        try
                        {
                            domain.Load(memory.ToArray());

                            // var a = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(fileName));
                            var a = AssemblyLoadContext.Default.LoadFromStream(memory);
                            var ret = a.GetType("Juvo.Program").GetMethod("Main").Invoke(null, null);
                            command.ResponseText = "Output:" +
                                ret == null || ret.ToString().Length == 0 ? "<no output>" : ret.ToString();
                        }
                        catch (Exception exc)
                        {
                            var msg = $"Error: {exc.Message}";
                            command.ResponseText = msg;
                            this.Log?.Error(msg, exc);
                        }
                        finally
                        {
                            AppDomain.Unload(domain);
                            GC.Collect();
                        }
                    }
                    else
                    {
                        foreach (var diag in results.Diagnostics)
                        {
                            command.ResponseText += $"[{diag.Id}] {diag.GetMessage()} ({diag.Location})";
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                var msg = $"Error: {exc.Message}";
                command.ResponseText = msg;
                this.Log?.Error(msg, exc);
            }

            // var cp = CodeDomProvider.CreateProvider("CSharp");
            // var res = cp.CompileAssemblyFromSource(
            //     new CompilerParameters { GenerateInMemory = true, MainClass = "Program" },
            // if (res.Errors.Count > 0)
            // {
            //     for (var i = 0; i < res.Errors.Count; ++i)
            //     {
            //         command.ResponseText += $"({i}) {res.Errors[i].ErrorText} ";
            //     }
            // }
            // else
            // {
            //     command.ResponseText = "Compiled successfully!";
            // }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Commands the bot to set specific settings.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandSet(IBotCommand command)
        {
            var parts = command.RequestText.Split(' ');

            if (parts.Length < 3)
            {
                command.ResponseText = $"{CommonResx.Usage}: set <setting> <value>";
            }

            switch (parts[1].ToLowerInvariant())
            {
                case "culture":
                    try
                    {
                        var cultureInfo = new CultureInfo(parts[2]);
                        Thread.CurrentThread.CurrentCulture = cultureInfo;
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;
                        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                        command.ResponseText = string.Format(SetResx.CultureChangedTo, cultureInfo.EnglishName);
                    }
                    catch (Exception exc)
                    {
                        var message = $"{SetResx.ErrorSettingCulture}: {exc.Message}";
                        this.log?.Error(message, exc);
                        command.ResponseText = message;
                    }

                    break;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Commands the bot to shutdown.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandShutdown(IBotCommand command)
        {
            this.log?.Info(InfoResx.ShuttingDown);
            await this.StopBots(CommonResx.Requested);
            if (this.config.WebServer.Enabled)
            {
                this.log?.Info(InfoResx.StoppingWebServer);
                await this.webServer.StopAsync(this.webHostToken);
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Commands the bot to return the status of the bot.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandStatus(IBotCommand command)
        {
            var status = new StringBuilder();

            if (this.ircBots.Any())
            {
                status.Append($"[{CommonResx.Irc}] ");
                foreach (var bot in this.ircBots)
                {
                    status.Append($"{bot.GetHashCode()} ");
                }
            }

            if (this.discordBots.Any())
            {
                status.Append($"[{CommonResx.Discord}] ");
                foreach (var bot in this.discordBots)
                {
                    status.Append($"{bot.GetHashCode()} ");
                }
            }

            if (this.slackBots.Any())
            {
                status.Append($"[{CommonResx.Slack}] ");
                foreach (var bot in this.slackBots)
                {
                    status.Append($"{bot.GetHashCode()} ");
                }
            }

            if (this.config.WebServer.Enabled)
            {
                status.Append($"[{CommonResx.WebServer}] {this.webServer.GetHashCode()}");
            }

            command.ResponseText = status.ToString();
            await Task.CompletedTask;
        }

        private void CreateAppFolders()
        {
            Directory.CreateDirectory(this.config.System.AppDataPath.FullName);
            Directory.CreateDirectory(this.config.System.LocalAppDataPath.FullName);
        }

        private void CreateResources()
        {
            this.CreateAppFolders();
        }

        private void LoadConfig()
        {
            foreach (var disc in this.config?.Discord?.Connections?.Where(x => x.Enabled))
            {
                var dc = new DiscordClient(
                    new ClientWebSocketProxy(),
                    new HttpClientProxy(new HttpClientHandlerProxy()),
                    new DiscordClientOptions { AuthToken = disc.AuthToken, IsBot = true });
                this.discordBots.Add(this.discordBotFactory.Create(disc, dc, this));
            }

            foreach (var irc in this.config?.Irc?.Connections?.Where(x => x.Enabled))
            {
                this.ircBots.Add(this.ircBotFactory.Create(irc, this));
            }

            foreach (var slack in this.config?.Slack?.Connections?.Where(x => x.Enabled))
            {
                this.slackBots.Add(this.slackBotFactory.Create(slack, this));
            }
        }

        private void LoadPlugins()
        {
            var scriptPath = Path.Combine(this.config.System.AppDataPath.FullName, "scripts");
            var binPath = Path.Combine(this.config.System.LocalAppDataPath.FullName, "bin");
            if (Directory.Exists(scriptPath))
            {
                var scripts = this.config.Juvo.Scripts
                    .Where(s => s.Enabled && File.Exists(Path.Combine(scriptPath, s.Script)));
                if (!scripts.Any())
                {
                    return;
                }

                if (!Directory.Exists(binPath))
                {
                    Directory.CreateDirectory(binPath);
                }

                foreach (var file in Directory.GetFiles(binPath))
                {
                    File.Delete(file);
                }

                foreach (var script in scripts)
                {
                    var stopWatch = Stopwatch.StartNew();
                    var scriptFile = new FileInfo(Path.Combine(scriptPath, script.Script));
                    var scriptCode = File.ReadAllText(scriptFile.FullName);
                    var scriptName = scriptFile.Name.Remove(scriptFile.Name.LastIndexOf('.'));
                    var assemblyPath = Path.Combine(binPath, $"{scriptName}.dll");
                    var compilation = CSharpCompilation.Create(scriptName)
                        .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                        .AddReferences(
                            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                            MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.0.5\System.Runtime.dll"),
                            MetadataReference.CreateFromFile(this.GetType().Assembly.Location))
                        .AddSyntaxTrees(CSharpSyntaxTree.ParseText(scriptCode));
                    var result = compilation.Emit(assemblyPath);

                    stopWatch.Stop();
                    if (!result.Success)
                    {
                        this.Log?.Warn($"Failed to compile script '{scriptName}':");
                        foreach (var diag in result.Diagnostics)
                        {
                            this.Log?.Warn($"  {diag}");
                        }

                        continue;
                    }

                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    this.Log?.Info($"[Plugin: {scriptName}] Compiled {scriptName} into assembly ({stopWatch.ElapsedMilliseconds}ms)");

                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || type.GetInterface(typeof(IBotPlugin).ToString(), true) == null)
                        {
                            this.Log?.Debug($"[Plugin: {scriptName}] Skipping {type.FullName}");
                            continue;
                        }

                        var instance = Activator.CreateInstance(type) as IBotPlugin;
                        if (instance == null)
                        {
                            this.Log?.Warn($"Could not instantiate plugin '{scriptName}'");
                            continue;
                        }

                        this.plugins.Add(instance.Commands.ToArray(), instance);
                        this.Log?.Info($"[Plugin: {scriptName}] Loaded!");
                        this.Log?.Info($"[Plugin: {scriptName}] Commands: {string.Join(", ", instance.Commands)}");
                    }
                }
            }
        }

        private void LogPerf()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                this.lastPerf =
                    $"[PRC] " +
                    $"Threads:{proc.Threads.Count} " +
                    $"Handles:{proc.HandleCount} " +
                    $"TPT:{proc.TotalProcessorTime.TotalSeconds:0.00}s " +
                    $"PPT:{proc.PrivilegedProcessorTime.TotalSeconds:0.00}s " +
                    $"UPT:{proc.TotalProcessorTime.TotalSeconds:0.00}s " +
                    $"[MEM] " +
                    $"WS:{this.ToMb(proc.WorkingSet64):0.00}mB " +
                    $"PM:{this.ToMb(proc.PrivateMemorySize64):0.00}mB " +
                    $"PS:{this.ToMb(proc.PagedSystemMemorySize64):0.00}mB " +
                    $"NP:{this.ToMb(proc.NonpagedSystemMemorySize64):0.00}mB " +
                    $"[INF] " +
                    $"PID:{proc.Id} " +
                    $"SID:{proc.SessionId} " +
                    $"Up:{DateTime.Now - proc.StartTime} " +
                    $"Taken:{DateTime.UtcNow} UTC";
                this.lastPerfTime = DateTime.UtcNow;
            }

            this.log?.Info(this.lastPerf);
        }

        private async Task ProcessCommand(IBotCommand cmd)
        {
            this.log?.Info(InfoResx.ProcessingCommand);
            var cmdName = cmd.RequestText.Split(' ')[0].ToLowerInvariant();

            var command = this.commands.SingleOrDefault(c => c.Key.Contains(cmdName));
            if (command.Value != null)
            {
                try
                {
                    await command.Value(cmd);
                }
                catch (Exception exc)
                {
                    var message = string.Format(InfoResx.ErrorExecCommand, command.Value.Method.Name);
                    this.log?.Error(message, exc);
                    cmd.ResponseText = message;
                }
            }
            else
            {
                var module = this.plugins.SingleOrDefault(p => p.Key.Contains(cmdName));
                if (module.Value != null)
                {
                    try
                    {
                        await Task.Factory.StartNew(() => module.Value.Execute(cmd));
                    }
                    catch (Exception exc)
                    {
                        var message = string.Format(InfoResx.ErrorExecCommand, module.Value.GetType().Name);
                        this.log?.Error(message, exc);
                        cmd.ResponseText = message;
                    }
                }
                else
                {
                    cmd.ResponseText = InfoResx.InvalidCommand;
                }
            }

            this.log?.Debug(DebugResx.EnqueuingResponse);
            if (cmd.Bot != null)
            {
                await cmd.Bot.QueueResponse(cmd);
            }
        }

        private async Task StartBots()
        {
            if (this.Config.Discord.Enabled)
            {
                await this.StartDiscordBots();
            }

            if (this.Config.Irc.Enabled)
            {
                await this.StartIrcBots();
            }

            if (this.Config.Slack.Enabled)
            {
                await this.StartSlackBots();
            }
        }

        private async Task StartDiscordBots()
        {
            foreach (var bot in this.discordBots)
            {
                await bot.Connect();
            }
        }

        private async Task StartIrcBots()
        {
            foreach (var bot in this.ircBots)
            {
                await bot.Connect();
            }
        }

        private async Task StartSlackBots()
        {
            foreach (var bot in this.slackBots)
            {
                await bot.Connect();
            }
        }

        private async Task StopBots(string quitMessage)
        {
            this.log?.Info(InfoResx.StoppingBots);

            foreach (var bot in this.discordBots)
            {
                await bot.Quit(quitMessage);
            }

            foreach (var bot in this.ircBots)
            {
                await bot.Quit(quitMessage);
            }

            foreach (var bot in this.slackBots)
            {
                await bot.Quit(quitMessage);
            }
        }

        private double ToMb(long bytes)
        {
            return bytes / (1024D * 1024D);
        }
    }
}
