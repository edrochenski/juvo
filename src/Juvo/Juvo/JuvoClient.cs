// <copyright file="JuvoClient.cs" company="https://gitlab.com/edrochenski/juvo">
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
    using JuvoProcess.Resources;
    using JuvoProcess.Resources.Commands;
    using JuvoProcess.Resources.Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.Extensions.FileProviders;
    using Newtonsoft.Json;

    /// <summary>
    /// Juvo client.
    /// </summary>
    public class JuvoClient : IJuvoClient
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
        private readonly IIrcBotFactory ircBotFactory;
        private readonly Mutex lastPerfLock;
        private readonly ILog log;
        private readonly Dictionary<string[], IBotPlugin> plugins;
        private readonly IServiceProvider serviceProvider;
        private readonly ISlackBotFactory slackBotFactory;
        private readonly DateTime started;
        private readonly IStorageHandler storageHandler;
        private readonly ManualResetEvent resetEvent;
        private readonly IWebHostBuilder webHostBuilder;
        private readonly CancellationToken webHostToken;

        private string lastPerf;
        private DateTime lastPerfTime;
        private List<JuvoUser> users;
        private IWebHost webHost;
        private bool webServerRunning;

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
            ManualResetEvent resetEvent = null)
        {
            this.Config = configuration ?? throw new ArgumentException(nameof(configuration));
            this.discordBotFactory = discordBotFactory ?? throw new ArgumentNullException(nameof(discordBotFactory));
            this.ircBotFactory = ircBotFactory ?? throw new ArgumentNullException(nameof(ircBotFactory));
            this.resetEvent = resetEvent ?? new ManualResetEvent(false);
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.slackBotFactory = slackBotFactory ?? throw new ArgumentNullException(nameof(slackBotFactory));
            this.storageHandler = storageHandler ?? throw new ArgumentNullException(nameof(storageHandler));
            this.webHostToken = default;
            this.webHostBuilder = webHostBuilder;

            this.bots = new List<IBot>();
            this.commandQueue = new Queue<IBotCommand>();
            this.commandTimer = new Timer(this.CommandTimerTick, null, TimerTickRate, TimerTickRate);
            this.lastPerfLock = new Mutex();
            this.log = logManager?.GetLogger(typeof(JuvoClient));
            this.plugins = new Dictionary<string[], IBotPlugin>();
            this.started = DateTime.UtcNow;
            this.State = JuvoState.Idle;
            this.webServerRunning = false;

            // this block maps the bots internal commands to the appropriate
            // internal methods.
            // ?: should these commands be overridable in some way, for example
            // if a plugin/module wanted to handle one of these.
            this.commands = new Dictionary<string[], Func<IBotCommand, Task>>
            {
                { new[] { "shutdown", "die" }, this.CommandShutdown },
                { new[] { "perf", "performance" }, this.CommandPerf },
                { new[] { "csc", "ros", "roslyn" }, this.CommandRoslyn },
                { new[] { "set" }, this.CommandSet },
                { new[] { "status" }, this.CommandStatus }
            };
        }

        /*/ Properties /*/

        /// <inheritdoc/>
        public Config Config { get; }

        /// <inheritdoc/>
        public ILog Log => this.log;

        /// <summary>
        /// Gets or sets the bot's current state.
        /// </summary>
        public JuvoState State { get; protected set; }

        /*/ Methods /*/

        /// <summary>
        /// Queues command for the bot to execute or pass on.
        /// </summary>
        /// <param name="cmd">Command object.</param>
        public void QueueCommand(IBotCommand cmd)
        {
            Debug.Assert(cmd != null, "cmd == null");
            Debug.Assert(!string.IsNullOrEmpty(cmd.RequestText), "cmd.RequestText != null/empty");

            lock (this)
            {
                this.commandQueue.Enqueue(cmd);
            }

            this.log?.Info(DebugResx.CommandEnqueued);
        }

        /// <inheritdoc/>
        public async Task Run()
        {
            this.log?.Info(InfoResx.CreatingMissingResources);
            this.CreateResources();

            this.log?.Info($"{InfoResx.LoadingUsers}...");
            this.LoadUsers();

            this.log?.Info(InfoResx.LoadingPlugins);
            this.LoadPlugins();

            this.log?.Info(InfoResx.LoadingConfigFile);
            this.LoadConfig();

            this.log?.Info(InfoResx.StartingBots);
            await this.StartBots();

            if (this.Config.WebServer.Enabled)
            {
                this.log?.Info(InfoResx.StartingWebServer);
                await this.StartWebServer();
            }
            else
            {
                this.log?.Warn(WarnResx.WebServerDisabled);
            }

            this.State = JuvoState.Running;
            this.log?.Info(InfoResx.BotRunning);
        }

        /// <summary>
        /// Called when <see cref="commandTimer" /> tick occurs.
        /// </summary>
        /// <param name="state">State object.</param>
        protected async void CommandTimerTick(object state)
        {
            if (this.commandQueue.Count > 0)
            {
                var toRun = new List<IBotCommand>();
                lock (this)
                {
                    IBotCommand cmd;
                    while (this.commandQueue.Count > 0 && (cmd = this.commandQueue.Dequeue()) != null)
                    {
                        toRun.Add(cmd);
                    }
                }

                await Task.Run(() => toRun.ForEach(async cmd => await this.ProcessCommand(cmd)));
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
                var binPath = Environment.ExpandEnvironmentVariables(Path.Combine(this.Config.Juvo.DataPath, "bin", "jr"));
                var ts = DateTime.UtcNow.Ticks.ToString();
                var name = $"jr{ts}";
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
                    $"      {string.Join(' ', command.RequestText.Replace("print(", "__outputBuilder__.Append(").Split(' ').Skip(1).ToArray())}{NL}" +
                    $"      return __outputBuilder__.ToString();{NL}" +
                    $"    }}{NL}" +
                    $"  }}{NL}" +
                    $"}}{NL}";

                if (!Directory.Exists(binPath))
                {
                    Directory.CreateDirectory(binPath);
                }

                var result = this.CompileAssembly(code, name, binPath);
                if (!result.Success)
                {
                    foreach (var diag in result.Diagnostics)
                    {
                        command.ResponseText += $"[{diag.Id}] {diag.GetMessage()} ({diag.Location})";
                    }

                    return;
                }

                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(binPath, $"{name}.dll"));
                var ret = assembly.GetType($"jr{ts}.Program").GetMethod("Main").Invoke(null, null);
                command.ResponseText = $"> {ret}";
            }
            catch (Exception exc)
            {
                var msg = $"Error: {exc.Message}";
                command.ResponseText = msg;
                this.Log?.Error(msg, exc);
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
            var quitMsg = CommonResx.Requested;
            var cmdTokens = command.RequestText.Split(' ');

            if (cmdTokens.Length > 1)
            {
                quitMsg = string.Join(' ', cmdTokens, 1, cmdTokens.Length - 1);
            }

            this.log?.Info(InfoResx.ShuttingDown);
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

        private EmitResult CompileAssembly(string code, string name, string assemblyPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(code), $"{nameof(code)} == null||empty");
            Debug.Assert(!string.IsNullOrEmpty(assemblyPath), $"{nameof(assemblyPath)} == null||empty");

            var assemblyFullPath = Path.Combine(assemblyPath, $"{name}.dll");
            var runtimeRef = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.5\" // HACK: This needs to go away,
                : "/usr/share/dotnet/shared/Microsoft.NETCore.App/2.2.5/";       // look into dotnet --list-runtimes

            try
            {
                var compilation = CSharpCompilation.Create(name)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "mscorlib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "netstandard.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Linq.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(runtimeRef, "System.Threading.Tasks.dll")),
                        MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(AngleSharp.Configuration).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(this.GetType().Assembly.Location))
                    .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));

                return compilation.Emit(assemblyFullPath);
            }
            catch (Exception exc)
            {
                this.log?.Error($"CompileAssembly(): {exc.Message}");
                return null;
            }
        }

        private void CreateResources()
        {
            this.storageHandler.DirectoryCreate(this.Config.Juvo.BasePath);
            this.storageHandler.DirectoryCreate(this.Config.Juvo.DataPath);

            var userFile = Environment.ExpandEnvironmentVariables(this.Config.Juvo.BasePath);
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
            if (this.Config == null)
            {
                return;
            }

            if (this.Config.Discord != null && this.Config.Discord.Enabled)
            {
                foreach (var disc in this.Config.Discord.Connections?.Where(x => x.Enabled))
                {
                    this.bots.Add(this.discordBotFactory.Create(disc, this.serviceProvider, this));
                }
            }

            if (this.Config.Irc != null && this.Config.Irc.Enabled)
            {
                foreach (var irc in this.Config.Irc.Connections?.Where(x => x.Enabled))
                {
                    this.bots.Add(this.ircBotFactory.Create(irc, this.serviceProvider, this));
                }
            }

            if (this.Config.Slack != null && this.Config.Slack.Enabled)
            {
                foreach (var slack in this.Config.Slack.Connections?.Where(x => x.Enabled))
                {
                    this.bots.Add(this.slackBotFactory.Create(slack, this.serviceProvider, this));
                }
            }
        }

        private void LoadPlugins()
        {
            var scriptPath = Environment.ExpandEnvironmentVariables(Path.Combine(this.Config.Juvo.BasePath, "scripts"));
            var binPath = Environment.ExpandEnvironmentVariables(Path.Combine(this.Config.Juvo.DataPath, "bin"));

            this.log?.Debug($"Script Path: {scriptPath}");
            this.log?.Debug($"Bin Path   : {binPath}");

            if (this.storageHandler.DirectoryExists(scriptPath))
            {
                var scripts = this.Config.Juvo.Scripts
                    .Where(s => s.Enabled && this.storageHandler.FileExists(Path.Combine(scriptPath, s.Script)));
                if (!scripts.Any())
                {
                    return;
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
                    var stopWatch = Stopwatch.StartNew();
                    var scriptFile = new FileInfo(Path.Combine(scriptPath, script.Script));
                    var scriptCode = this.storageHandler.FileReadAllText(scriptFile.FullName);
                    var scriptName = scriptFile.Name.Remove(scriptFile.Name.LastIndexOf('.'));
                    var assemblyPath = Path.Combine(binPath, $"{scriptName}.dll");

                    var result = this.CompileAssembly(scriptCode, scriptName, binPath);
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

        private void LoadUsers()
        {
            var userFile = Environment.ExpandEnvironmentVariables(this.Config.Juvo.BasePath);
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
                        await module.Value.Execute(cmd);
                    }
                    catch (Exception exc)
                    {
                        var message = string.Format(InfoResx.ErrorExecCommand, module.Value.GetType().Name);
                        this.log?.Error(message, exc.InnerException ?? exc);
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
            var startTasks = new List<Task>();

            if (this.Config.Discord.Enabled)
            {
                foreach (var bot in this.bots.Where(b => b.Type == BotType.Discord))
                {
                    startTasks.Add(bot.Connect());
                }
            }

            if (this.Config.Irc.Enabled)
            {
                foreach (var bot in this.bots.Where(b => b.Type == BotType.Irc))
                {
                    startTasks.Add(bot.Connect());
                }
            }

            if (this.Config.Slack.Enabled)
            {
                foreach (var bot in this.bots.Where(b => b.Type == BotType.Slack))
                {
                    startTasks.Add(bot.Connect());
                }
            }

            await Task.WhenAll(startTasks);
        }

        private async Task StartWebServer()
        {
            if (this.webServerRunning)
            {
                return;
            }

            this.webHost = this.BuildWebHost();
            await this.webHost?.RunAsync(this.webHostToken);
            this.webServerRunning = true;
        }

        private async Task StopBots(string quitMessage)
        {
            this.log?.Info(InfoResx.StoppingBots);

            var stopTasks = new List<Task>();
            foreach (var bot in this.bots)
            {
                stopTasks.Add(bot.Quit(quitMessage));
            }

            await Task.WhenAll(stopTasks);
        }

        private async Task StopWebServer()
        {
            if (!this.webServerRunning)
            {
                return;
            }

            this.log?.Info(InfoResx.StoppingWebServer);
            await this.webHost.StopAsync(this.webHostToken);
        }

        private double ToMb(long bytes)
        {
            return bytes / (1024D * 1024D);
        }
    }
}
