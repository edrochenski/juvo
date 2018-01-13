// <copyright file="IrcBot.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Bots
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using JuvoProcess.Configuration;
    using JuvoProcess.Net.Irc;
    using log4net;

    /// <summary>
    /// IRC bot.
    /// </summary>
    public class IrcBot : IIrcBot
    {
/*/ Constants /*/
        private const string DefaultCommandToken = ".";

/*/ Fields /*/
        private readonly IrcConfigConnection config;
        private readonly IJuvoClient host;
        private string commandToken;
        private IrcClient client;
        private bool isDisposed;
        private ILog log;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcBot"/> class.
        /// </summary>
        /// <param name="host">Host of the juvo client.</param>
        /// <param name="config">IRC configuration.</param>
        public IrcBot(IrcConfigConnection config, IJuvoClient host)
        {
            this.commandToken = config.CommandToken ?? DefaultCommandToken;
            this.config = config;
            this.host = host;
            this.log = LogManager.GetLogger(typeof(IrcBot));

            this.client = new IrcClient(IrcClient.LookupNetwork(this.config.Network))
            {
                NickName = config.Nickname ?? throw new Exception("Nickname is missing from configuration"),
                NickNameAlt = config.NicknameAlt ?? $"{config.Nickname}-",
                RealName = config.RealName ?? string.Empty,
                Username = config.Ident ?? config.Nickname.ToLowerInvariant()
            };

            this.client.ChannelJoined += this.Client_ChannelJoined;
            this.client.ChannelMessage += this.Client_ChannelMessage;
            this.client.ChannelModeChanged += this.Client_ChannelModeChanged;
            this.client.ChannelParted += this.Client_ChannelParted;
            this.client.Connected += this.Client_Connected;
            this.client.Disconnected += this.Client_Disconnected;
            this.client.HostHidden += this.Client_HostHidden;
            this.client.MessageReceived += this.Client_MessageReceived;
            this.client.PrivateMessage += this.Client_PrivateMessage;
            this.client.UserModeChanged += this.Client_UserModeChanged;
            this.client.UserQuit += this.Client_UserQuit;
        }

/*/ Properties /*/

        /// <summary>
        /// Gets or sets a value indicating whether the bot has authenticated.
        /// </summary>
        public bool IsAuthenticated { get; protected set; }

        /// <inheritdoc/>
        public BotType Type { get => BotType.Irc; }

/*/ Methods /*/

    // Public

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Connect()
        {
            this.log.Debug("Connecting");
            this.client.Connect(this.config.Servers.First().Host, this.config.Servers.First().Port);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public Task QueueResponse(IBotCommand cmd)
        {
            if (cmd is IrcBotCommand ircCmd && !string.IsNullOrEmpty(cmd.ResponseText))
            {
                this.client.SendMessage(ircCmd.Channel, ircCmd.ResponseText);
            }

            return Task.CompletedTask;
        }

    // Protected

        /// <summary>
        /// Initiates the authentication process.
        /// </summary>
        protected void Authenticate()
        {
            if (this.IsAuthenticated)
            {
                this.log.Error("Authenticate(): Bot has already authenticated");
                return;
            }
            else if (string.IsNullOrEmpty(this.config.User)
                || string.IsNullOrEmpty(this.config.Pass)
                || string.IsNullOrEmpty(this.config.Network))
            {
                this.log.Error("Authenticate(): config missing user, pass, or network");
                return;
            }

            switch (this.client.Network)
            {
                case IrcNetwork.Undernet:
                {
                    this.log.Info("Authenticating with X@ on undernet...");
                    this.client.SendMessage(
                        $"x@channels.undernet.org", $"login {this.config.User} {this.config.Pass}");
                    break;
                }
            }
        }

        /// <summary>
        /// Processes a join command.
        /// </summary>
        /// <param name="cmdArgs">Command args.</param>
        protected virtual void CommandJoin(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2 || this.client.CurrentChannels.Contains(cmdArgs[1]))
            {
                return;
            }

            if (!cmdArgs[1].Contains(","))
            {
                if (cmdArgs.Length > 2)
                {
                    this.client.Join(cmdArgs[1], cmdArgs[2]);
                }
                else
                {
                    this.client.Join(cmdArgs[1]);
                }
            }
            else
            {
                string[] chans = cmdArgs[1].Split(',');
                string[] keys = (cmdArgs.Length > 2) ? cmdArgs[2].Split(',') : null;

                if (keys == null || chans.Length == keys.Length)
                {
                    this.log.Debug($"Joining {string.Join(", ", chans)}");
                    this.client.Join(chans, keys);
                }
            }
        }

        /// <summary>
        /// Processes a powershell command.
        /// </summary>
        /// <param name="cmdArgs">Command args.</param>
        protected virtual void CommandPowershell(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2)
            {
                return;
            }

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

                    // proc.EnableRaisingEvents = true;
                    // proc.Exited += (sender, e) =>
                    // {
                    //     logger?.LogTrace($"CommandPowershell(): Process exited");
                    //     client.SendMessage("#bytedown", $"proc exited");
                    // };
                    // proc.ErrorDataReceived += (sender, e) =>
                    // {
                    //     logger?.LogWarning($"CommandPowershell(): Error: {e.Data}");
                    //     client.SendMessage("#bytedown", $"error: {e.Data}");
                    // };
                    // proc.OutputDataReceived += (sender, e) =>
                    // {
                    //     logger?.LogTrace($"CommandPowershell(): Output: {e.Data}");
                    //     client.SendMessage("#bytedown", $"output: {e.Data}");
                    // };
                    this.log.Debug($"CommandPowershell(): Starting with args [{args}]");
                    if (!proc.Start())
                    {
                        this.log.Debug($"CommandPowershell(): Trying to start a process that may be already started?");
                        this.client.SendMessage("#bytedown", $"not started: is something already running?");
                    }
                    else
                    {
                        this.log.Debug($"CommandPowershell(): Process running, id: {proc.Id}");

                        var error = proc.StandardError.ReadToEnd();
                        var output = proc.StandardOutput.ReadToEnd();
                        if (proc.WaitForExit(10000))
                        {
                            if (proc.ExitCode != 0)
                            {
                                this.log.Debug($"CommandPowershell(): Error: {error}");
                                this.client.SendMessage("#bytedown", $"Error: {error}");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(output))
                                {
                                    this.log.Debug($"CommandPowershell(): Output: {output.Trim()}");
                                    this.client.SendMessage("#bytedown", $"Output: {output.Trim()}");
                                }
                            }
                        }
                        else
                        {
                            this.log.Debug("CommandPowershell(): Timed out");
                            this.client.SendMessage("#bytedown", "Output: Timed out");
                        }
                    }
                }
                catch (Exception exc)
                {
                    this.log.Error($"Exception running Powershell: {exc.Message}", exc);
                    this.client.SendMessage("#bytedown", $"exception: {exc.Message}");
                }
            }
        }

        /// <summary>
        /// Processes a quit command.
        /// </summary>
        /// <param name="cmdArgs">Command args.</param>
        protected virtual void CommandQuit(string[] cmdArgs)
        {
            var msg = (cmdArgs.Length > 1)
                ? string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1)
                : string.Empty;
            this.client.Quit(msg);
        }

        /// <summary>
        /// Processes a raw command.
        /// </summary>
        /// <param name="cmdArgs">Command args.</param>
        protected virtual void CommandRaw(string[] cmdArgs)
        {
            if (cmdArgs.Length == 1)
            {
                return;
            }

            var rawMessage = string.Join(" ", cmdArgs, 1, cmdArgs.Length - 1);
            this.client.Send($"{rawMessage}{IrcClient.CrLf}");
        }

        /// <summary>
        /// Processes a twitter command.
        /// </summary>
        /// <param name="cmdArgs">Command args.</param>
        protected virtual void CommandTwitter(string[] cmdArgs)
        {
            if (cmdArgs.Length < 2)
            {
                return;
            }

            // Broken due to 'Configuration' name-clash with AngleSharp
            // var handle = cmdArgs[1];
            // var config = Configuration.Default.WithDefaultLoader();
            // var address = $"https://twitter.com/{handle}";
            // var document = BrowsingContext.New(config).OpenAsync(address);
        }

        /// <summary>
        /// Clean up object and release resources.
        /// </summary>
        /// <param name="isDisposing">Is is currently disposing.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            try
            {
                if (!this.isDisposed && isDisposing)
                {
                }
            }
            finally
            {
                this.isDisposed = true;
            }
        }

    // Private
        private void Client_ChannelJoined(object sender, ChannelUserEventArgs e)
        {
            this.log.Info($"{e.User.Nickname} joined {e.Channel}");
        }

        private void Client_ChannelMessage(object sender, ChannelUserEventArgs e)
        {
            this.log.Debug($"<{e.Channel}\\{e.User.Nickname}> {e.Message}");
            if (e.Message.StartsWith(this.config.CommandToken))
            {
                this.host.QueueCommand(new IrcBotCommand
                {
                    Bot = this,
                    Channel = e.Channel,
                    RequestText = e.Message.Remove(0, this.config.CommandToken.Length)
                });

                // var cmdParts = e.Message.Split(' ');
                // switch (cmdParts[0].Replace(config.CommandToken, "").ToLowerInvariant())
                // {
                //     case "join":    CommandJoin(cmdParts); break;
                //     case "psh":     CommandPowershell(cmdParts); break;
                //     case "quit":    CommandQuit(cmdParts); break;
                //     case "raw":     CommandRaw(cmdParts); break;
                //     case "twitter": CommandTwitter(cmdParts); break;
                // }
            }
        }

        private void Client_ChannelModeChanged(object sender, ChannelModeChangedEventArgs e)
        {
            var message = new StringBuilder($"{e.Channel} mode changed: ");

            if (e.Added.Count() > 0)
            {
                message.Append("+[");

                var count = 0;
                foreach (var mode in e.Added)
                {
                    if (++count > 1)
                    {
                        message.Append(", ");
                    }

                    message.Append(mode.Mode);

                    if (!string.IsNullOrEmpty(mode.Value))
                    {
                        message.Append($"({mode.Value})");
                    }
                }

                message.Append("] ");
            }

            if (e.Removed.Count() > 0)
            {
                message.Append("-[");

                var count = 0;
                foreach (var mode in e.Removed)
                {
                    if (++count > 1)
                    {
                        message.Append(", ");
                    }

                    message.Append(mode.Mode);

                    if (!string.IsNullOrEmpty(mode.Value))
                    {
                        message.Append($"({mode.Value})");
                    }
                }

                message.Append("]");
            }

            this.log.Info(message.ToString());
        }

        private void Client_ChannelParted(object sender, ChannelUserEventArgs e)
        {
            this.log.Info($"{e.User.Nickname} parted {e.Channel}");
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            this.log.Info($"Connected to server");

            if (!string.IsNullOrEmpty(this.config.UserMode))
            {
                this.log.Info($"Requeting mode: +{this.config.UserMode}");
                this.client.Send($"MODE {this.client.NickName} +{this.config.UserMode}{IrcClient.CrLf}");
            }

            if (!string.IsNullOrEmpty(this.config.User)
                && !string.IsNullOrEmpty(this.config.Pass)
                && !string.IsNullOrEmpty(this.config.Network))
            {
                this.Authenticate();
            }
            else
            {
                this.JoinAllChannels();
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            this.log.Info($"Disconnected from server");
        }

        private void Client_HostHidden(object sender, HostHiddenEventArgs e)
        {
            this.log.Info($"Real host hidden using '{e.Host}'");
            if (this.config.Network.ToLowerInvariant() == "undernet")
            {
                this.IsAuthenticated = true;
                this.JoinAllChannels();
            }
        }

        private void Client_MessageReceived(object sender, MessageReceivedArgs e)
        {
            this.log.Debug($"MSG: {e.Message}");
        }

        private void Client_PrivateMessage(object sender, UserEventArgs e)
        {
            this.log.Debug($"<PRIVMSG\\{e.User.Nickname}> {e.Message}");
        }

        private void Client_UserModeChanged(object sender, UserModeChangedEventArgs e)
        {
            this.log.Info(
                $"User mode changed: +[{string.Join(" ", e.Added)}] -[{string.Join(" ", e.Removed)}]");
        }

        private void Client_UserQuit(object sender, UserEventArgs e)
        {
            this.log.Debug($"{e.User.Nickname} quit");
        }

        private void JoinAllChannels()
        {
            if (this.config != null && this.config.Channels != null)
            {
                foreach (var chan in this.config.Channels)
                {
                    this.client.Join(chan.Name);
                }
            }
        }
    }
}
