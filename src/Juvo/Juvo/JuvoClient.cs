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
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using JuvoProcess.Bots;
    using JuvoProcess.Configuration;
    using JuvoProcess.IO;
    using JuvoProcess.Juvo;
    using JuvoProcess.Net;
    using JuvoProcess.Resources;
    using JuvoProcess.Resources.Commands;
    using JuvoProcess.Resources.Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Juvo client.
    /// </summary>
    public class JuvoClient : IJuvoClient, IDisposable
    {
        /*/ Constants /*/

        private const int TimerTickRate = 100;
        private const string UserFileName = "users.json";

        /*/ Static /*/

        private static readonly string NL = Environment.NewLine;

        /*/ Fields /*/

        private readonly List<IBot> bots;
        private readonly Queue<IBotCommand> commandQueue;
        private readonly Dictionary<string[], Func<IBotCommand, Task>> commands;
        private readonly Timer commandTimer;
        private readonly IDiscordBotFactory discordBotFactory;
        private readonly IHttpClient httpClient;
        private readonly IIrcBotFactory ircBotFactory;
        private readonly object lastPerfLock;
        private readonly Dictionary<string[], IBotPlugin> plugins;
        private readonly IServiceProvider serviceProvider;
        private readonly ISlackBotFactory slackBotFactory;
        private readonly DateTime started;
        private readonly IStorageHandler storageHandler;
        private readonly ManualResetEvent resetEvent;
        private readonly IWebHostBuilder webHostBuilder;
        private readonly CancellationToken webHostToken;

        private int commandTimerLastMin = -1;
        private bool isDisposed;
        private string lastPerf = string.Empty;
        private DateTime lastPerfTime;
        private List<JuvoUser>? users;
        private IWebHost? webHost;
        private bool webServerRunning;
        private Thread? webServerThread;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="JuvoClient"/> class.
        /// </summary>
        /// <param name="configuration">Bot's configuration.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="discordBotFactory">Factory object for Discord bots.</param>
        /// <param name="ircBotFactory">Factory object for IRC bots.</param>
        /// <param name="slackBotFactory">Factory object for Slack bots.</param>
        /// <param name="logManager">Log manager.</param>
        /// <param name="webHostBuilder">Webhost builder object for creating a web server.</param>
        /// <param name="storageHandler">Storage handler for working with files/dirs.</param>
        /// <param name="httpClient">HTTP client instance for web-based requests.</param>
        /// <param name="resetEvent">Manual reset object for thread.</param>
        public JuvoClient(
            Config configuration,
            IServiceProvider serviceProvider,
            IDiscordBotFactory discordBotFactory,
            IIrcBotFactory ircBotFactory,
            ISlackBotFactory slackBotFactory,
            ILogManager logManager,
            IWebHostBuilder webHostBuilder,
            IStorageHandler storageHandler,
            IHttpClient httpClient,
            ManualResetEvent? resetEvent = null)
        {
            this.Config            = configuration ?? throw new ArgumentException(nameof(configuration));
            this.discordBotFactory = discordBotFactory ?? throw new ArgumentNullException(nameof(discordBotFactory));
            this.httpClient        = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.ircBotFactory     = ircBotFactory ?? throw new ArgumentNullException(nameof(ircBotFactory));
            this.resetEvent        = resetEvent ?? new ManualResetEvent(false);
            this.serviceProvider   = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.slackBotFactory   = slackBotFactory ?? throw new ArgumentNullException(nameof(slackBotFactory));
            this.storageHandler    = storageHandler ?? throw new ArgumentNullException(nameof(storageHandler));
            this.webHostToken      = default;
            this.webHostBuilder    = webHostBuilder;

            this.bots = new List<IBot>();
            this.commandQueue = new Queue<IBotCommand>();
            this.commandTimer = new Timer(this.CommandTimerTick, null, TimerTickRate, TimerTickRate);
            this.lastPerfLock = new Mutex();
            this.Logger = logManager?.GetLogger(typeof(JuvoClient));
            this.plugins = new Dictionary<string[], IBotPlugin>();
            this.started = DateTime.UtcNow;
            this.State = JuvoState.Idle;
            this.webServerRunning = false;

            this.httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.0 Safari/537.36 Edg/89.0.774.4");

            // this block maps the bots internal commands to the appropriate
            // internal methods.
            // ?: should these commands be overridable in some way, for example
            // if a plugin/module wanted to handle one of these.
            this.commands = new Dictionary<string[], Func<IBotCommand, Task>>
            {
                { new[] { "echo" }, this.CommandEcho },
                { new[] { "shutdown", "die" }, this.CommandShutdown },
                { new[] { "perf", "performance" }, this.CommandPerf },
                { new[] { "csc", "ros", "roslyn" }, this.CommandRoslyn },
                { new[] { "set" }, this.CommandSet },
                { new[] { "status" }, this.CommandStatus }
            };
        }

        /*/ Properties /*/

        /// <inheritdoc/>
        public List<IBot> Bots => this.bots;

        /// <inheritdoc/>
        public Config Config { get; }

        /// <inheritdoc/>
        public IHttpClient HttpClient => this.httpClient;

        /// <summary>
        /// Gets or sets the bot's current state.
        /// </summary>
        public JuvoState State { get; protected set; }

        /// <summary>
        /// Gets the log to use.
        /// </summary>
        protected ILog? Logger { get; }

        /*/ Methods /*/

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, object message, Exception? exception = null)
        {
            if (this.Logger is null) { return; }

            switch (level)
            {
                case LogLevel.Critical:     this.Logger.Fatal(message, exception); break;
                case LogLevel.Debug:        this.Logger.Debug(message, exception); break;
                case LogLevel.Error:        this.Logger.Error(message, exception); break;
                case LogLevel.Information:  this.Logger.Info(message, exception); break;
                case LogLevel.None:         break;
                case LogLevel.Trace:        this.Logger.Debug(message, exception); break;
                case LogLevel.Warning:      this.Logger.Warn(message, exception); break;
            }
        }

        /// <summary>
        /// Queues command for the bot to execute or pass on.
        /// </summary>
        /// <param name="cmd">Command object.</param>
        public void QueueCommand(IBotCommand cmd)
        {
            Debug.Assert(cmd != null, $"{nameof(cmd)} == null");
            Debug.Assert(!string.IsNullOrEmpty(cmd.RequestText), $"{nameof(cmd.RequestText)} != null/empty");

            lock (this.commandQueue)
            {
                this.commandQueue.Enqueue(cmd);
            }

            this.Logger?.Info(DebugResx.CommandEnqueued);
        }

        /// <inheritdoc/>
        public async Task QueueResponse(IBotCommand cmd)
        {
            this.Logger?.Debug(DebugResx.EnqueuingResponse);
            if (cmd.Bot != null)
            {
                await cmd.Bot.QueueResponse(cmd);
            }
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            this.CreateResources();
            this.LoadUsers();
            this.LoadConfig();

            if (!this.LoadPlugins() && this.Config.Juvo != null && this.Config.Juvo.StopOnCompileErrors)
            {
                Environment.Exit(-1);
                return;
            }

            await this.StartBots();

            if (this.Config.WebServer != null && this.Config.WebServer.Enabled)
            {
                this.webServerThread = new Thread(this.StartWebServer) { Name = "WebServer" };
                this.webServerThread.Start();
            }
            else
            {
                this.Logger?.Warn(WarnResx.WebServerDisabled);
            }

            this.State = JuvoState.Running;
            this.Logger?.Info(InfoResx.BotRunning);
        }

        /// <summary>
        /// Command for the bot to echo back the request.
        /// </summary>
        /// <param name="command">Bot command.</param>
        /// <returns>A Task object associated with the async operation.</returns>
        protected async Task CommandEcho(IBotCommand command)
        {
            command.ResponseText = command.RequestText.Replace(command.RequestText.Split(' ')[0], string.Empty);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called when <see cref="commandTimer" /> tick occurs.
        /// </summary>
        /// <param name="state">State object.</param>
        protected async void CommandTimerTick(object? state)
        {
            if (this.State != JuvoState.Running) { return; }

            if (this.commandQueue.Count > 0)
            {
                var toRun = new List<IBotCommand>();
                lock (this.commandQueue)
                {
                    IBotCommand cmd;
                    while (this.commandQueue.Count > 0 && (cmd = this.commandQueue.Dequeue()) != null)
                    {
                        toRun.Add(cmd);
                    }
                }

                await Task.Run(() => toRun.ForEach(async cmd => await this.ProcessCommand(cmd)));
            }

            if (this.commandTimerLastMin != DateTime.Now.Minute && this.plugins.Count > 0)
            {
                this.commandTimerLastMin = DateTime.Now.Minute;
                foreach (var plugin in this.plugins.Values)
                {
                    if (plugin.CommandTimeMin is null || !plugin.CommandTimeMin.Contains(DateTime.Now.Minute))
                    {
                        continue;
                    }

                    var src = new CommandSource { SourceType = CommandSourceType.None };
                    var cmd = new BotCommand(null, CommandTriggerType.Timer, src, string.Empty);

                    await plugin.Execute(cmd, this); // nocommit: how do we know where to send the unsolicited response??
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
                            if (!int.TryParse(cmdTokens[2], out var threadNumber))
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
            var tokens = command.RequestText.Split(' ');
            if (tokens.Length == 1)
            {
                command.ResponseText = $"USAGE: {tokens[0]} <code>";
                return;
            }

            try
            {
                var binPath = Environment.ExpandEnvironmentVariables(Path.Combine(this.Config.Juvo?.DataPath ?? string.Empty, "bin", "jr"));
                var ts = DateTime.UtcNow.Ticks.ToString();
                var name = $"jr{ts}";
                var inject = command.RequestText
                    .Replace("print(", "__outputBuilder__.Append(")
                    .Replace("Console.Write(", "__outputBuilder__.Append(")
                    .Replace("Console.WriteLine(", "__outputBuilder__.Append(")
                    .Split(' ').Skip(1).ToArray();
                var code =
                    $"#pragma warning disable SA1118{NL}" +
                    $"namespace {name}{NL}" +
                    $"{{{NL}" +
                    $"  using System;{NL}" +
                    $"  using System.Text;{NL}" +
                    $"  using JuvoProcess;{NL}" +
                    $"  public class Program{NL}" +
                    $"  {{{NL}" +
                    $"    public static string Main(){NL}" +
                    $"    {{{NL}" +
                    $"      var __outputBuilder__ = new StringBuilder();{NL}" +
                    $"      {string.Join(' ', inject)}{NL}" +
                    $"      return __outputBuilder__.ToString();{NL}" +
                    $"    }}{NL}" +
                    $"  }}{NL}" +
                    $"}}{NL}";

                if (!Directory.Exists(binPath))
                {
                    Directory.CreateDirectory(binPath);
                }

                var result = this.CompileAssembly(code, name, binPath);
                if (result == null)
                {
                    return;
                }

                if (!result.Success)
                {
                    if (result.Diagnostics.Length > 2)
                    {
                        var diag = result.Diagnostics[0];
                        command.ResponseText = $"{diag.Id}: {diag.GetMessage()} .. (+{result.Diagnostics.Length - 1} more errors)";
                    }
                    else
                    {
                        command.ResponseText = string.Empty;
                        foreach (var diag in result.Diagnostics)
                        {
                            if (command.ResponseText.Length > 0)
                            {
                                command.ResponseText += " | ";
                            }

                            command.ResponseText += $"{diag.Id}: {diag.GetMessage()}";
                        }
                    }

                    return;
                }

                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(binPath, $"{name}.dll"));
                    var ret = assembly.GetType($"jr{ts}.Program")?.GetMethod("Main")?.Invoke(null, null) as string;
                    command.ResponseText = $"> {(string.IsNullOrEmpty(ret) ? "(no output)" : ret)}";
                }
                catch (Exception exc)
                {
                    command.ResponseText = $"! {exc.Message}";
                }
            }
            catch (Exception exc)
            {
                var msg = $"Error: {exc.Message}";
                command.ResponseText = msg;
                this.Logger?.Error(msg, exc);
            }

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
                        this.Logger?.Error(message, exc);
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
            var quitMsg = CommonResx.Requested;
            var cmdTokens = command.RequestText.Split(' ');

            if (cmdTokens.Length > 1)
            {
                quitMsg = string.Join(' ', cmdTokens, 1, cmdTokens.Length - 1);
            }

            this.Logger?.Info(InfoResx.ShuttingDown);
            await this.StopBots(quitMsg);
            await this.StopWebServer();

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
            var lastType = BotType.Unknown;

            // bots
            foreach (var bot in this.bots.OrderBy(b => b.Type))
            {
                if (bot.Type != lastType)
                {
                    status.Append($"[{bot.Type}] ");
                    lastType = bot.Type;
                }

                status.Append($"{bot.GetHashCode()} ");
            }

            // web server
            var webState = this.webServerRunning ? CommonResx.Running : CommonResx.Stopped;
            status.Append($"[{CommonResx.WebServer}] {webState}");

            command.ResponseText = status.ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// Disposes of any resources being used by this instance.
        /// </summary>
        /// <param name="isDisposing">Was dispose explicitly called.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (!this.isDisposed && isDisposing)
            {
                this.commandTimer?.Dispose();
                this.resetEvent?.Dispose();
                this.webHost?.Dispose();
            }

            this.isDisposed = true;
        }

        private IWebHost BuildWebHost()
        {
            var builder = new WebHostBuilder();
            return builder
                .UseContentRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"))
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000); // TODO: config for ip/ports
                })
                .Configure(cfg =>
                {
                    cfg.UseDefaultFiles(new DefaultFilesOptions
                    {
                        DefaultFileNames = new List<string> { "index.html" },
                        FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "wwwroot")),
                        RequestPath = string.Empty
                    });
                    cfg.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "wwwroot")),
                        RequestPath = string.Empty
                    });
                })
                .Build();
        }

        private EmitResult? CompileAssembly(string code, string name, string assemblyPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(code), $"{nameof(code)} == null||empty");
            Debug.Assert(!string.IsNullOrEmpty(assemblyPath), $"{nameof(assemblyPath)} == null||empty");

            try
            {
                // HACK: This needs to go away, look into `dotnet --info, --version, --list-runtimes`
                var assemblyFullPath = Path.Combine(assemblyPath, $"{name}.dll");
                var debugFullPath = Path.Combine(assemblyPath, $"{name}.pdb");
                var runtimeRef = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.3\"
                    : @"/usr/share/dotnet/shared/Microsoft.NETCore.App/5.0.3/";

                var compilation = CSharpCompilation.Create(name)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "mscorlib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "netstandard.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Linq.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Net.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Net.Http.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Net.Primitives.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Private.Uri.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Threading.Tasks.dll")),
                        MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(AngleSharp.Configuration).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(AngleSharp.XPath.Extensions).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(JsonConvert).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(LogLevel).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(this.GetType().Assembly.Location))
                    .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

                return compilation.Emit(assemblyFullPath, debugFullPath);
            }
            catch (FileNotFoundException exc)
            {
                this.Logger?.Error("This exception was most likely caused by referencing a missing version of the .NETCore runtime");
                this.Logger?.Error($"CompileAssembly(): {exc.Message}");
                return null;
            }
            catch (Exception exc)
            {
                this.Logger?.Error($"CompileAssembly(): {exc.Message}");
                return null;
            }
        }

        private void CreateResources()
        {
            this.Logger?.Info(InfoResx.CreatingMissingResources);

            if (this.Config.Juvo?.BasePath != null)
            { this.storageHandler.DirectoryCreate(this.Config.Juvo.BasePath); }

            if (this.Config.Juvo?.DataPath != null)
            { this.storageHandler.DirectoryCreate(this.Config.Juvo.DataPath); }

            var userFile = Environment.ExpandEnvironmentVariables(this.Config.Juvo?.BasePath ?? string.Empty);
            userFile = Path.Combine(userFile, UserFileName);
            if (!this.storageHandler.FileExists(userFile))
            {
                var guid = Guid.NewGuid();
                var adminUser = new JuvoUser
                {
                    Id = guid.ToString(),
                    Username = "admin",
                    Password = "password".GenerateSaltedHash(guid.ToString()).ToBase64()
                };
                var data = JsonConvert.SerializeObject(new List<JuvoUser> { adminUser });
                this.storageHandler.FileWriteAllText(userFile, data);
            }
        }

        private void LoadConfig()
        {
            this.Logger?.Info(InfoResx.LoadingConfigFile);

            if (this.Config.Discord != null && this.Config.Discord.Enabled)
            {
                if (this.Config.Discord.Connections != null && this.Config.Discord.Connections.Any())
                {
                    foreach (var disc in this.Config.Discord.Connections.Where(x => x.Enabled))
                    {
                        this.bots.Add(this.discordBotFactory.Create(disc, this.serviceProvider, this));
                    }
                }
            }

            if (this.Config.Irc != null && this.Config.Irc.Enabled)
            {
                if (this.Config.Irc.Connections != null && this.Config.Irc.Connections.Any())
                {
                    foreach (var irc in this.Config.Irc.Connections.Where(x => x.Enabled))
                    {
                        this.bots.Add(this.ircBotFactory.Create(irc, this.serviceProvider, this));
                    }
                }
            }

            if (this.Config.Slack != null && this.Config.Slack.Enabled)
            {
                if (this.Config.Slack.Connections != null && this.Config.Slack.Connections.Any())
                {
                    foreach (var slack in this.Config.Slack.Connections.Where(x => x.Enabled))
                    {
                        this.bots.Add(this.slackBotFactory.Create(slack, this.serviceProvider, this));
                    }
                }
            }
        }

        private bool LoadPlugins()
        {
            var loadResult = true;

            this.Logger?.Info(InfoResx.LoadingPlugins);

            var scriptPath = Environment.ExpandEnvironmentVariables(Path.Combine(this.Config.Juvo?.BasePath ?? string.Empty, "scripts"));
            var binPath = Environment.ExpandEnvironmentVariables(Path.Combine(this.Config.Juvo?.DataPath ?? string.Empty, "bin"));

            this.Logger?.Debug($"Script Path: {scriptPath}");
            this.Logger?.Debug($"Bin Path   : {binPath}");

            if (this.storageHandler.DirectoryExists(scriptPath))
            {
                var scripts = this.Config.Juvo?.Scripts?
                    .Where(s => s.Enabled && this.storageHandler.FileExists(Path.Combine(scriptPath, s.Script ?? string.Empty)));
                if (scripts == null || !scripts.Any())
                {
                    return loadResult;
                }

                if (!this.storageHandler.DirectoryExists(binPath))
                {
                    this.storageHandler.DirectoryCreate(binPath);
                }

                foreach (var file in this.storageHandler.DirectoryGetFiles(binPath))
                {
                    this.storageHandler.FileDelete(file);
                }

                foreach (var script in scripts)
                {
                    if (script.Script is null) { continue; }

                    var stopWatch = Stopwatch.StartNew();
                    var scriptFile = new FileInfo(Path.Combine(scriptPath, script.Script));
                    var scriptCode = this.storageHandler.FileReadAllText(scriptFile.FullName);
                    var scriptName = scriptFile.Name.Remove(scriptFile.Name.LastIndexOf('.'));
                    var assemblyPath = Path.Combine(binPath, $"{scriptName}.dll");

                    // TODO: wrap this process in a try-catch
                    var result = this.CompileAssembly(scriptCode, scriptName, binPath);
                    stopWatch.Stop();

                    if (result != null && !result.Success)
                    {
                        loadResult = false;

                        this.Logger?.Warn($"Failed to compile script '{scriptName}':");
                        foreach (var diag in result.Diagnostics)
                        {
                            this.Logger?.Warn($"  {diag}");
                        }

                        continue;
                    }

                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                    this.Logger?.Info($"[Plugin: {scriptName}] Compiled {scriptName} into assembly ({stopWatch.ElapsedMilliseconds}ms)");

                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsAbstract || type.GetInterface(typeof(IBotPlugin).ToString(), true) == null)
                        {
                            this.Logger?.Debug($"[Plugin: {scriptName}] Skipping {type.FullName}");
                            continue;
                        }

                        var instance = Activator.CreateInstance(type) as IBotPlugin;
                        if (instance == null)
                        {
                            loadResult = false;
                            this.Logger?.Warn($"Could not instantiate plugin '{scriptName}'");
                            continue;
                        }

                        this.plugins.Add(instance.Commands.ToArray(), instance);
                        this.Logger?.Info($"[Plugin: {scriptName}] Loaded!");
                        this.Logger?.Info($"[Plugin: {scriptName}] Commands: {string.Join(", ", instance.Commands)}");
                    }
                }
            }

            return loadResult;
        }

        private void LoadUsers()
        {
            this.Logger?.Info($"{InfoResx.LoadingUsers}...");

            var userFile = Environment.ExpandEnvironmentVariables(this.Config.Juvo?.BasePath ?? string.Empty);
            userFile = Path.Combine(userFile, UserFileName);
            if (!this.storageHandler.FileExists(userFile))
            {
                var userData = this.storageHandler.FileReadAllText(userFile);
                this.users = JsonConvert.DeserializeObject<List<JuvoUser>>(userData);
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

            this.Logger?.Info(this.lastPerf);
        }

        private async Task ProcessCommand(IBotCommand cmd)
        {
            this.Logger?.Info(InfoResx.ProcessingCommand);
            var cmdName = cmd.RequestText.Split(' ')[0].ToLowerInvariant();

            var command = this.commands.SingleOrDefault(c => c.Key.Contains(cmdName));
            if (command.Value != null)
            {
                try
                {
                    this.Logger?.Info(InfoResx.ExecutingBuiltinCommand);
                    await command.Value(cmd);
                }
                catch (Exception exc)
                {
                    var message = string.Format(InfoResx.ErrorExecCommand, command.Value.Method.Name);
                    this.Logger?.Error(message, exc);
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
                        this.Logger?.Info(string.Format(InfoResx.ExecutingPluginCommand, module.Value.GetType().Name));
                        await module.Value.Execute(cmd, this);
                    }
                    catch (Exception exc)
                    {
                        var message = string.Format(InfoResx.ErrorExecCommand, module.Value.GetType().Name);
                        this.Logger?.Error(message, exc.InnerException ?? exc);
                        cmd.ResponseText = message;
                    }
                }
                else
                {
                    cmd.ResponseText = InfoResx.InvalidCommand;
                }
            }

            await this.QueueResponse(cmd);
        }

        private async Task StartBots()
        {
            this.Logger?.Info(InfoResx.StartingBots);

            var startTasks = new List<Task>();

            if (this.Config.Discord != null && this.Config.Discord.Enabled)
            {
                foreach (var bot in this.bots.Where(b => b.Type == BotType.Discord))
                {
                    startTasks.Add(bot.Connect());
                }
            }

            if (this.Config.Irc != null && this.Config.Irc.Enabled)
            {
                foreach (var bot in this.bots.Where(b => b.Type == BotType.Irc))
                {
                    startTasks.Add(bot.Connect());
                }
            }

            if (this.Config.Slack != null && this.Config.Slack.Enabled)
            {
                foreach (var bot in this.bots.Where(b => b.Type == BotType.Slack))
                {
                    startTasks.Add(bot.Connect());
                }
            }

            await Task.WhenAll(startTasks);
        }

        private void StartWebServer()
        {
            if (this.webServerRunning)
            {
                return;
            }

            this.Logger?.Info(InfoResx.StartingWebServer);

            this.webHost = this.BuildWebHost();
            this.webHost.Run();
            this.webServerRunning = true;
        }

        private async Task StopBots(string quitMessage)
        {
            this.Logger?.Info(InfoResx.StoppingBots);

            var stopTasks = new List<Task>();
            foreach (var bot in this.bots)
            {
                stopTasks.Add(bot.Quit(quitMessage));
            }

            await Task.WhenAll(stopTasks);
        }

        private async Task StopWebServer()
        {
            Debug.Assert(this.webHost != null && this.webServerRunning, "Stop called on stopped web server");

            if (!this.webServerRunning || this.webHost is null)
            {
                return;
            }

            this.Logger?.Info(InfoResx.StoppingWebServer);
            await this.webHost.StopAsync(this.webHostToken);
        }

        private double ToMb(long bytes)
        {
            return bytes / (1024D * 1024D);
        }
    }
}
