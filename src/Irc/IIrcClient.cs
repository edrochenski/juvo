using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace juvo.Irc
{
    public interface IIrcClient
    {
        event EventHandler<ChannelUserEventArgs> ChannelJoined;
        event EventHandler<ChannelUserEventArgs> ChannelMessage;
        event EventHandler Connected;
        event EventHandler DataReceived;
        event EventHandler MessageReceived;
        event EventHandler<IrcReply> ReplyReceived;

        List<string> CurrentChannels { get; }

        Task ConnectAsync(string serverHost, int serverPort);
        Task SendAsync(string data);
        Task SendAsync(string format, params object[] args);
        Task SendAsync(byte[] data);
        Task SendMessageAsync(string to, string format, params object[] args);
    }
}
