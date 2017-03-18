using AngleSharp;
using BytedownSoftware.Lib;
using Microsoft.Extensions.Logging;
using JuvoConsole;
using System;
using System.Linq;
using System.Diagnostics;

namespace Juvo.Net.Irc
{
    public class IrcBot
    {
    /*/ Constants /*/
        public const string DefaultCommandToken = ".";

    /*/ Fields /*/
        readonly ILoggerFactory loggerFactory;

        string              commandToken;
        IrcConfigConnection config;
        IrcClient           client;
        ILogger             logger;

    /*/ Properties /*/
        public bool IsAuthenticated { get; protected set; }

    /*/ Constructors /*/
        public IrcBot(IrcConfigConnection config, ILoggerFactory loggerFactory = null)
        {
            this.config    = config;
            this.loggerFactory = loggerFactory;

            if (loggerFactory != null)
            { logger = loggerFactory.CreateLogger<IrcBot>(); }

            commandToken = config.CommandToken ?? DefaultCommandToken;

            client = new IrcClient(IrcClient.LookupNetwork(this.config.Network), loggerFactory)
            {
                NickName = config.Nickname ?? throw new Exception("Nickname is missing from configuration"),
                NickNameAlt = config.NicknameAlt ?? throw new Exception("NicknameAlt is missing from configuration"),
                RealName = config.RealName ?? "",
                Username = config.Ident ?? config.Nickname
            };
            client.ChannelJoined += Client_ChannelJoined;
            client.ChannelMessage += Client_ChannelMessage;
            client.ChannelParted += Client_ChannelParted;
            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;
            client.HostHidden += Client_HostHidden;
            client.MessageReceived += Client_MessageReceived;
            client.PrivateMessage += Client_PrivateMessage;
            client.UserQuit += Client_UserQuit;
        }

    /*/ Public Methods /*/
        public void Connect()
        {
            LogTrace("Connecting");
            client.Connect(config.Servers.First().Host, config.Servers.First().Port);
        }

    /*/ Protected Methods /*/
        protected void Authenticate()
        {
            if (IsAuthenticated)
            {
                LogWarning("Authenticate(): Bot has already authenticated");
                return;
            }
            else if (StringUtil.AreAnyMissing(config.User, config.Pass, config.Network))
            {
                LogWarning("Authenticate(): config missing user, pass, or network");
                return;
            }

            switch (client.Network)
            {
                case IrcNetwork.Undernet:
                {
                    LogInfo("Authenticating with X@ on undernet...");
                    client.SendMessage($"x@channels.undernet.org", $"login {config.User} {config.Pass}");
                } break;

            }
        }
        protected virtual void CommandJoin(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2) { return; }
            if (client.CurrentChannels.Contains(cmdArgs[1])) { return; }

            if (!cmdArgs[1].Contains(","))
            {
                if (cmdArgs.Length > 2)
                { client.Join(cmdArgs[1], cmdArgs[2]); }
                else
                { client.Join(cmdArgs[1]); }
            }
            else
            {
                string[] chans = cmdArgs[1].Split(',');
                string[] keys  = (cmdArgs.Length > 2) ? cmdArgs[2].Split(',') : null;

                if (keys == null || chans.Length == keys.Length)
                {
                    LogTrace($"Joining {string.Join(", ", chans)}");
                    client.Join(chans, keys);
                }
            }
        }
        protected virtual void CommandPowershell(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2) { return; }

            var args = string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1);
            var info = new ProcessStartInfo()
            {
                Arguments = $"-NoLogo -NoProfile -NonInteractive -WindowStyle Hidden -Command {args}",
                CreateNoWindow = true,
                FileName = "powershell",
                LoadUserProfile = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            using (var proc = new Process())
            {
                try
                {
                    proc.StartInfo = info;
                    //proc.EnableRaisingEvents = true;
                    //proc.Exited += (sender, e) =>
                    //{
                    //    logger?.LogTrace($"CommandPowershell(): Process exited");
                    //    client.SendMessage("#bytedown", $"proc exited");
                    //};
                    //proc.ErrorDataReceived += (sender, e) => 
                    //{
                    //    logger?.LogWarning($"CommandPowershell(): Error: {e.Data}");
                    //    client.SendMessage("#bytedown", $"error: {e.Data}");
                    //};
                    //proc.OutputDataReceived += (sender, e) =>
                    //{
                    //    logger?.LogTrace($"CommandPowershell(): Output: {e.Data}");
                    //    client.SendMessage("#bytedown", $"output: {e.Data}");
                    //};

                    LogDebug($"CommandPowershell(): Starting with args [{args}]");
                    if (!proc.Start())
                    {
                        logger?.LogWarning($"CommandPowershell(): Trying to start a process that may be already started?");
                        client.SendMessage("#bytedown", $"not started: is something already running?");
                    }
                    else
                    {
                        LogDebug($"CommandPowershell(): Process running, id: {proc.Id}");

                        var error = proc.StandardError.ReadToEnd();
                        var output = proc.StandardOutput.ReadToEnd();
                        if (proc.WaitForExit(10000))
                        {
                            if (proc.ExitCode != 0)
                            {
                                LogTrace($"CommandPowershell(): Error: {error}");
                                client.SendMessage("#bytedown", $"Error: {error}");
                            }
                            else
                            {
                                if (!StringUtil.IsMissing(output))
                                {
                                    LogTrace($"CommandPowershell(): Output: {output.Trim()}");
                                    client.SendMessage("#bytedown", $"Output: {output.Trim()}");
                                }
                            }
                        }
                        else
                        {
                            LogWarning("CommandPowershell(): Timed out");
                            client.SendMessage("#bytedown", "Output: Timed out");
                        }
                    }
                }
                catch (Exception exc)
                {
                    LogError($"CommandPowershell(): {exc.Message}");
                    client.SendMessage("#bytedown", $"exception: {exc.Message}");
                }
            }
        }
        protected virtual void CommandQuit(string[] cmdArgs)
        {
            var msg = (cmdArgs.Length > 1) ? string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1) : "";
            client.Quit(msg);
        }
        protected virtual void CommandRaw(string[] cmdArgs)
        {
            if (cmdArgs.Length == 1) { return; }

            var rawMessage = string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1);
            client.Send($"{rawMessage}{IrcClient.CrLf}");
        }
        protected virtual void CommandTwitter(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2) { return; }

            var handle = cmdArgs[1];
            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://twitter.com/{handle}";
            var document = BrowsingContext.New(config).OpenAsync(address);
        }

    /*/ Private Methods /*/
        void Client_ChannelJoined(object sender, ChannelUserEventArgs e)
        {
            LogInfo($"{e.User.Nickname} joined {e.Channel}");
        }
        void Client_ChannelMessage(object sender, ChannelUserEventArgs e)
        {
            LogDebug($"<{e.Channel}\\{e.User.Nickname}> {e.Message}");
            
            if (e.Channel.ToLowerInvariant().Equals("#bytedown") && e.Message.StartsWith(config.CommandToken))
            {
                var cmdParts = e.Message.Split(' ');
                switch (cmdParts[0].Replace(config.CommandToken, "").ToLowerInvariant())
                {
                    case "join":    CommandJoin(cmdParts); break;
                    case "psh":     CommandPowershell(cmdParts); break;
                    case "quit":    CommandQuit(cmdParts); break;
                    case "raw":     CommandRaw(cmdParts); break;
                    case "twitter": CommandTwitter(cmdParts); break;
                }
            }
        }
        void Client_ChannelParted(object sender, ChannelUserEventArgs e)
        {
            LogInfo($"{e.User.Nickname} parted {e.Channel}");
        }
        void Client_Connected(object sender, EventArgs e)
        {
            LogInfo($"Connected to server");
            
            if (!StringUtil.IsMissing(config.UserMode))
            {
                LogInfo($"Requeting mode: +{config.UserMode}");
                client.Send($"MODE {client.NickName} +{config.UserMode}{IrcClient.CrLf}");
            }

            if (!StringUtil.AreAnyMissing(config.User, config.Pass, config.Network))
            {
                Authenticate();
            }
            else
            {
                JoinAllChannels();
            }
        }
        void Client_Disconnected(object sender, EventArgs e)
        {
            LogInfo($"Disconnected from server");
        }
        void Client_HostHidden(object sender, HostHiddenEventArgs e)
        {
            LogInfo($"Real host hidden using '{e.Host}'");
            if (config.Network.ToLowerInvariant() == "undernet")
            {
                IsAuthenticated = true;
                JoinAllChannels();
            }
        }
        void Client_MessageReceived(object sender, MessageReceivedArgs e)
        {
            LogTrace($"MSG: {e.Message}");
        }
        void Client_PrivateMessage(object sender, UserEventArgs e)
        {
            LogDebug($"<PRIVMSG\\{e.User.Nickname}> {e.Message}");
        }
        void Client_UserQuit(object sender, UserEventArgs e)
        {
            LogDebug($"{e.User.Nickname} quit");
        }
        void JoinAllChannels()
        {
            if (config != null && config.Channels != null)
            {
                foreach (var chan in config.Channels)
                { client.Join(chan.Name); }
            }
        }
        void LogCritical(string message) { logger?.LogCritical($"[{config.Name}] {message}"); }
        void LogDebug(string message) { logger?.LogDebug($"[{config.Name}] {message}"); }
        void LogError(string message) { logger?.LogError($"[{config.Name}] {message}"); }
        void LogInfo(string message) { logger?.LogInformation($"[{config.Name}] {message}"); }
        void LogTrace(string message) { logger?.LogTrace($"[{config.Name}] {message}"); }
        void LogWarning(string message) { logger?.LogWarning($"[{config.Name}] {message}"); }
    }
}
