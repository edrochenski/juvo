using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juvo.Net.Irc
{
    public interface IIrcClient
    {
        event EventHandler<ChannelUserEventArgs> ChannelJoined;
        event EventHandler<ChannelUserEventArgs> ChannelMessage;
        event EventHandler<ChannelUserEventArgs> ChannelParted;
        event EventHandler Connected;
        event EventHandler DataReceived;
        event EventHandler Disconnected;
        event EventHandler<MessageReceivedArgs> MessageReceived;
        event EventHandler<UserEventArgs> PrivateMessage;
        event EventHandler<IrcReply> ReplyReceived;
        event EventHandler<UserEventArgs> UserQuit;

        List<string> CurrentChannels { get; }
        string CurrentNickname { get; }
        string NickName { get; set; }
        string NickNameAlt { get; set; }
        string RealName { get; set; }
        string Username { get; set; }

        void Connect(string serverHost, int serverPort);
        void Join(string channel);
        void Send(string data);
        void Send(string format, params object[] args);
        void Send(byte[] data);
        void SendMessage(string to, string format, params object[] args);
    }
}
