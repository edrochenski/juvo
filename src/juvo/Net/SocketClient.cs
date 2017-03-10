using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Juvo.Net
{
    public class SocketClient : IDisposable
    {
    /*/ Constants /*/
        public const int DefaultBufferSize = 4096;

    /*/ Events /*/
        public event EventHandler<EventArgs> ConnectCompleted;
        public event EventHandler<EventArgs> ConnectFailed;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<ReceiveCompletedEventArgs> ReceiveCompleted;
        public event EventHandler<SocketEventArgs> ReceiveFailed;
        public event EventHandler<EventArgs> SendCompleted;
        public event EventHandler<SocketEventArgs> SendFailed;

    /*/ Fields /*/
        SocketAsyncEventArgs argsConnect;
        SocketAsyncEventArgs argsReceive;
        SocketAsyncEventArgs[] argsSend;
        byte[]               dataBuffer;
        string               host;
        int                  port;
        DnsEndPoint          remoteEndPoint;
        Socket               socket;

    /*/ Constructors /*/
        public SocketClient() : this(DefaultBufferSize) { }
        public SocketClient(int bufferSize)
        {
            dataBuffer = new byte[bufferSize];
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            argsConnect = new SocketAsyncEventArgs();
            argsConnect.Completed += SocketConnect_Completed;
            argsReceive = new SocketAsyncEventArgs();
            argsReceive.Completed += SocketReceive_Completed;
            argsReceive.SetBuffer(dataBuffer, 0, dataBuffer.Length);

            argsSend = new SocketAsyncEventArgs[5];
            for (int i = 0; i < argsSend.Length; ++i)
            {
                argsSend[i] = new SocketAsyncEventArgs();
                argsSend[i].Completed += SocketSend_Completed;
            }

        }

    /*/ Public Methods /*/
        public void Connect(string host, int port)
        {
            this.host = host;
            this.port = port;

            remoteEndPoint = new DnsEndPoint(host, port);
            argsConnect.RemoteEndPoint = remoteEndPoint;
            argsReceive.RemoteEndPoint = remoteEndPoint;

            foreach (var sae in argsSend)
            { sae.RemoteEndPoint = remoteEndPoint; }

            Connect();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }
        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (socket != null) socket.Dispose();
            }
        }
        public void Send(string text)
        {
            Send(Encoding.UTF8.GetBytes(text));
        }
        public void Send(byte[] bytes)
        {
            var sendArgs = GetSendArgFromPool();

            sendArgs.SetBuffer(bytes, 0, bytes.Length);

            if (!socket.SendAsync(sendArgs))
            { SocketSend_Completed(this, sendArgs); }
        }

        /*/ Private Methods /*/
        void Connect()
        {
            if (!socket.ConnectAsync(argsConnect))
            { SocketConnect_Completed(this, argsConnect); }
        }
        SocketAsyncEventArgs GetSendArgFromPool()
        {
            var result = argsSend.First(x => x.UserToken == null);
            result.UserToken = DateTime.UtcNow;
            return result;
        }
        void Receive()
        {
            Debug.WriteLine($"[SocketClient] In Receive()");
            argsReceive.SetBuffer(0, DefaultBufferSize);
            if (!socket.ReceiveAsync(argsReceive))
            { SocketReceive_Completed(this, argsReceive); }
        }
        void ReturnSendArgToPool(SocketAsyncEventArgs args)
        {
            args.UserToken = null;
        }
        void SocketConnect_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                ConnectCompleted?.Invoke(this, EventArgs.Empty);
                Receive();
            }
            else
            {
                ConnectFailed?.Invoke(this, EventArgs.Empty);
            }
        }
        void SocketReceive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    Debug.WriteLine($"[SocketClient] Received {e.BytesTransferred} bytes...");
                    var data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, data, e.BytesTransferred);
                    ReceiveCompleted?.Invoke(this, new ReceiveCompletedEventArgs(e.BytesTransferred, data));

                    Receive();
                }
                else
                {
                    Disconnected?.Invoke(this, EventArgs.Empty);
                    socket.Shutdown(SocketShutdown.Both);
                }
            }
            else
            {
                ReceiveFailed?.Invoke(this, new SocketEventArgs(e.SocketError, e.SocketError.ToString()));
            }
        }
        void SocketSend_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Debug.WriteLine($"[SocketClient] Send completed: {Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred)}");
                SendCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SendFailed?.Invoke(this, new SocketEventArgs(e.SocketError, e.SocketError.ToString()));
            }
            ReturnSendArgToPool(e);
        }
    }

    public class ReceiveCompletedEventArgs : EventArgs
    {
        public byte[] Data;
        public int Length;

        public ReceiveCompletedEventArgs(int length, byte[] data)
        {
            this.Data = data;
            this.Length = length;
        }
    }
    public class SocketEventArgs : EventArgs
    {
        public SocketError Error;
        public string ErrorText;

        public SocketEventArgs(SocketError error, string errorText)
        {
            this.Error = error;
            this.ErrorText = errorText;
        }
    }
}
