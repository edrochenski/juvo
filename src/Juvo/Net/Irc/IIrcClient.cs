// <copyright file="IIrcClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net.Irc
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a way to connect to an IRC client.
    /// </summary>
    public interface IIrcClient
    {
        /*/ Events /*/

        /// <summary>
        /// Event raised when a channel is joined.
        /// </summary>
        event EventHandler<ChannelUserEventArgs> ChannelJoined;

        /// <summary>
        /// Event raised when a channel message arrives.
        /// </summary>
        event EventHandler<ChannelUserEventArgs> ChannelMessage;

        /// <summary>
        /// Event raised when a channel changes modes.
        /// </summary>
        event EventHandler<ChannelModeChangedEventArgs> ChannelModeChanged;

        /// <summary>
        /// Event raised when a user parts a channel.
        /// </summary>
        event EventHandler<ChannelUserEventArgs> ChannelParted;

        /// <summary>
        /// Event raised when connected.
        /// </summary>
        event EventHandler Connected;

        //// <summary>
        //// Event raised when data is received.
        //// </summary>
        // event EventHandler DataReceived;

        /// <summary>
        /// Event raised when disconnected.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Event raised when the host is hidden.
        /// </summary>
        event EventHandler<HostHiddenEventArgs> HostHidden;

        /// <summary>
        /// Event raised when message received.
        /// </summary>
        event EventHandler<MessageReceivedArgs> MessageReceived;

        /// <summary>
        /// Event raised when a private message arrives.
        /// </summary>
        event EventHandler<UserEventArgs> PrivateMessage;

        /// <summary>
        /// Event raised when an IRC reply is received.
        /// </summary>
        event EventHandler<IrcReply> ReplyReceived;

        /// <summary>
        /// Event raised when a user's mode is changed.
        /// </summary>
        event EventHandler<UserModeChangedEventArgs> UserModeChanged;

        /// <summary>
        /// Event raised when a user quits.
        /// </summary>
        event EventHandler<UserEventArgs> UserQuit;

        /*/ Properties /*/

        /// <summary>
        /// Gets the current channels.
        /// </summary>
        List<string> CurrentChannels { get; }

        /// <summary>
        /// Gets the current nickname.
        /// </summary>
        string CurrentNickname { get; }

        /// <summary>
        /// Gets the network.
        /// </summary>
        IrcNetwork Network { get; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        /// Gets or sets the alternate nickname.
        /// </summary>
        string NickNameAlt { get; set; }

        /// <summary>
        /// Gets or sets the real name.
        /// </summary>
        string RealName { get; set; }

        /// <summary>
        /// Gets the user modes.
        /// </summary>
        IEnumerable<IrcUserMode> UserModes { get; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        string Username { get; set; }

        /*/ Methods /*/

        /// <summary>
        /// Connects to the IRC server.
        /// </summary>
        /// <param name="serverHost">Host to connect to.</param>
        /// <param name="serverPort">Port to connect to.</param>
        /// <param name="serverPassword">Password for server/network.</param>
        void Connect(string serverHost, int serverPort, string serverPassword);

        /// <summary>
        /// Joins a channel.
        /// </summary>
        /// <param name="channel">Channel to join.</param>
        void Join(string channel);

        /// <summary>
        /// Joins a channel.
        /// </summary>
        /// <param name="channel">Channel to join.</param>
        /// <param name="key">Key for channel, defaults to <see cref="string.Empty"/></param>
        void Join(string channel, string key = "");

        /// <summary>
        /// Joins channels.
        /// </summary>
        /// <param name="channels">Channels to join.</param>
        /// <param name="channelKeys">Keys for channels.</param>
        void Join(string[] channels, string[] channelKeys = null);

        /// <summary>
        /// Looks up a user mode.
        /// </summary>
        /// <param name="mode">Mode to resolve.</param>
        /// <returns>Resolved user mode.</returns>
        IrcUserMode LookupUserMode(char mode);

        /// <summary>
        /// Looks up multiple user modes.
        /// </summary>
        /// <param name="mode">Modes to look up.</param>
        /// <returns>Resolved modes.</returns>
        IEnumerable<IrcUserMode> LookupUserModes(char[] mode);

        /// <summary>
        /// Looks up multiple user modes.
        /// </summary>
        /// <param name="mode">Modes to look up.</param>
        /// <returns>Resolved modes.</returns>
        IEnumerable<IrcUserMode> LookupUserModes(string mode);

        /// <summary>
        /// Parts a channel.
        /// </summary>
        /// <param name="channel">Channel to part.</param>
        /// <param name="message">Part message.</param>
        void Part(string channel, string message = "");

        /// <summary>
        /// Quit/disconnect from server.
        /// </summary>
        /// <param name="message">Quit message.</param>
        void Quit(string message = "");

        /// <summary>
        /// Sends data to the server.
        /// </summary>
        /// <param name="data">Data to be sent.</param>
        void Send(string data);

        /// <summary>
        /// Sends data to the server.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args for format string.</param>
        void Send(string format, params object[] args);

        /// <summary>
        /// Sends data to the server.
        /// </summary>
        /// <param name="data">Data to send.</param>
        void Send(byte[] data);

        /// <summary>
        /// Send message to the server.
        /// </summary>
        /// <param name="to">Target of the message.</param>
        /// <param name="format">Format string.</param>
        /// <param name="args">Args for format string.</param>
        void SendMessage(string to, string format, params object[] args);
    }
}
