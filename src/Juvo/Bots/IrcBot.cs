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

    /// <summary>
    /// IRC bot.
    /// </summary>
    public class IrcBot : IIrcBot
    {
        /*/ Constants /*/

        private const string DefaultCommandToken = ".";
        private const int Debug = 0;
        private const int Info = 1;
        private const int Warn = 2;
        private const int Error = 3;
        private const int Fatal = 4;

        /*/ Fields /*/

        private readonly IIrcClient client;
        private readonly ILogManager logManager;

        private string commandToken;
        private IrcConfigConnection config;
        private IJuvoClient host;
        private bool isDisposed;
        private ILog log;

        /*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcBot"/> class.
        /// </summary>
        /// <param name="logManager">Log manager.</param>
        /// <param name="ircClient">IRC client.</param>
        public IrcBot(ILogManager logManager, IIrcClient ircClient)
        {
            this.logManager = logManager;
            this.client = ircClient;

            this.log = logManager?.GetLogger(typeof(IrcBot));
        }

        /*/ Properties /*/

        /// <inheritdoc/>
        public string CurrentNickname => this.client.CurrentNickname;

        /// <summary>
        /// Gets or sets a value indicating whether the bot has authenticated.
        /// </summary>
        public bool IsAuthenticated { get; protected set; }

        /// <inheritdoc/>
        public IrcNetwork Network => this.client.Network;

        /// <inheritdoc/>
        public BotType Type { get => BotType.Irc; }

        /*/ Methods /*/

        /// <summary>
        /// Starts the connection process.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Connect()
        {
            this.Log(Info, "Connecting");
            await Task.Run(() =>
                this.client.Connect(
                    this.config.Servers.First().Host,
                    this.config.Servers.First().Port,
                    this.config.NetworkToken));
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
            if (/*cmd is IrcBotCommand ircCmd && */!string.IsNullOrEmpty(cmd.ResponseText))
            {
                this.client.SendMessage(cmd.Source.Identifier, cmd.ResponseText);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task Quit(string message)
        {
            this.Log(Info, $"Quitting: {message ?? "(null)"}");
            await Task.Run(() => this.client.Quit(message));
        }

        /// <inheritdoc/>
        public void Initialize(IrcConfigConnection config, IJuvoClient juvoClient)
        {
            this.config = config;
            this.host = juvoClient;

            this.client.Network = IrcClient.LookupNetwork(this.config.Network);
            this.client.NickName = config.Nickname ?? throw new Exception("Nickname is missing from configuration");
            this.client.NickNameAlt = config.NicknameAlt ?? $"{config.Nickname}-";
            this.client.RealName = config.RealName ?? string.Empty;
            this.client.Username = config.Ident ?? config.Nickname.ToLowerInvariant();

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
            this.commandToken = config.CommandToken ?? DefaultCommandToken;
        }

        /// <summary>
        /// Initiates the authentication process.
        /// </summary>
        protected void Authenticate()
        {
            if (this.IsAuthenticated)
            {
                this.Log(Error, "Authenticate(): Bot has already authenticated");
                return;
            }
            else if (string.IsNullOrEmpty(this.config.User)
                || string.IsNullOrEmpty(this.config.Pass)
                || string.IsNullOrEmpty(this.config.Network))
            {
                this.Log(Error, "Authenticate(): config missing user, pass, or network");
                return;
            }

            switch (this.client.Network)
            {
                case IrcNetwork.Undernet:
                {
                    this.Log(Info, "Authenticating with X@ on undernet...");
                    this.client.SendMessage(
                        $"x@channels.undernet.org", $"login {this.config.User} {this.config.Pass}");
                    break;
                }
            }
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

        private void Client_ChannelJoined(object sender, ChannelUserEventArgs e)
        {
            this.Log(Debug, $"{e.User.Nickname} joined {e.Channel}");
        }

        private void Client_ChannelMessage(object sender, ChannelUserEventArgs e)
        {
            if (e.Message.StartsWith(this.config.CommandToken))
            {
                this.Log(Info, $"<{e.Channel}\\{e.User.Nickname}> {e.Message}");
                this.host.QueueCommand(new IrcBotCommand
                {
                    Bot = this,
                    RequestText = e.Message.Remove(0, this.config.CommandToken.Length),
                    Source = new CommandSource
                    {
                        Identifier = e.Channel,
                        SourceType = CommandSourceType.ChannelOrGroup
                    }
                });
            }
            else
            {
                this.Log(Debug, $"<{e.Channel}\\{e.User.Nickname}> {e.Message}");
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

            this.Log(Debug, message.ToString());
        }

        private void Client_ChannelParted(object sender, ChannelUserEventArgs e)
        {
            this.Log(Debug, $"{e.User.Nickname} parted {e.Channel}");
        }

        private void Client_Connected(object sender, EventArgs e)
        {
            this.Log(Info, $"Connected to server");

            if (!string.IsNullOrEmpty(this.config.UserMode))
            {
                this.Log(Debug, $"Requeting mode: +{this.config.UserMode}");
                this.client.Send($"MODE {this.client.NickName} +{this.config.UserMode}{IrcClient.CrLf}");
            }

            if (!string.IsNullOrEmpty(this.config.User)
                && !string.IsNullOrEmpty(this.config.Pass))
            {
                this.Authenticate();
            }

            if (!this.config.JoinOnHostMasked)
            {
                this.JoinAllChannels();
            }
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            this.Log(Info, $"Disconnected from server");
        }

        private void Client_HostHidden(object sender, HostHiddenEventArgs e)
        {
            this.IsAuthenticated = true;
            this.Log(Info, $"Real host hidden using '{e.Host}'");

            if (this.config.JoinOnHostMasked)
            {
                this.JoinAllChannels();
            }
        }

        private void Client_MessageReceived(object sender, MessageReceivedArgs e)
        {
            this.Log(Debug, $"MSG: {e.Message}");
        }

        private void Client_PrivateMessage(object sender, UserEventArgs e)
        {
            this.Log(Debug, $"<PRIVMSG\\{e.User.Nickname}> {e.Message}");
        }

        private void Client_UserModeChanged(object sender, UserModeChangedEventArgs e)
        {
            this.Log(Debug, $"User mode changed: +[{string.Join(" ", e.Added)}] -[{string.Join(" ", e.Removed)}]");
        }

        private void Client_UserQuit(object sender, UserEventArgs e)
        {
            this.Log(Debug, $"{e.User.Nickname} quit");
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

        private void Log(int level, string text = null, Exception exc = null)
        {
            if (this.log == null) { return; }

            var qualifiedText = $"[{this.config.Name}] {text}";

            switch (level)
            {
                case Debug: this.log.Debug(qualifiedText, exc); break;
                case Info: this.log.Info(qualifiedText, exc); break;
                case Warn: this.log.Warn(qualifiedText, exc); break;
                case Error: this.log.Error(qualifiedText, exc); break;
                case Fatal: this.log.Fatal(qualifiedText, exc); break;
                default: throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}
