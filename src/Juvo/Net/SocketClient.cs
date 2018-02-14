// <copyright file="SocketClient.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

namespace JuvoProcess.Net
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// Socket client.
    /// </summary>
    public class SocketClient : IDisposable
    {
/*/ Constants /*/
        private const int DefaultBufferSize = 4096;

/*/ Fields /*/
        private SocketAsyncEventArgs argsConnect;
        private SocketAsyncEventArgs argsReceive;
        private SocketAsyncEventArgs[] argsSend;
        private byte[] dataBuffer;
        private bool disposed;
        private string host;
        private int port;
        private DnsEndPoint remoteEndPoint;
        private Socket socket;

/*/ Constructors /*/

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        public SocketClient()
            : this(DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        /// <param name="bufferSize">Size of buffer.</param>
        public SocketClient(int bufferSize)
        {
            this.disposed = false;
            this.dataBuffer = new byte[bufferSize];
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            this.argsConnect = new SocketAsyncEventArgs();
            this.argsConnect.Completed += this.SocketConnect_Completed;
            this.argsReceive = new SocketAsyncEventArgs();
            this.argsReceive.Completed += this.SocketReceive_Completed;
            this.argsReceive.SetBuffer(this.dataBuffer, 0, this.dataBuffer.Length);

            this.argsSend = new SocketAsyncEventArgs[5];
            for (int i = 0; i < this.argsSend.Length; ++i)
            {
                this.argsSend[i] = new SocketAsyncEventArgs();
                this.argsSend[i].Completed += this.SocketSend_Completed;
            }
        }

/*/ Events /*/

        /// <summary>
        /// Fires when a connection has completed successfully.
        /// </summary>
        public event EventHandler<EventArgs> ConnectCompleted;

        /// <summary>
        /// Fires when a connection attempt has failed.
        /// </summary>
        public event EventHandler<EventArgs> ConnectFailed;

        /// <summary>
        /// Fires when the client is disconnected.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Fires when the current receive process is completed.
        /// </summary>
        public event EventHandler<ReceiveCompletedEventArgs> ReceiveCompleted;

        /// <summary>
        /// Fires when the current receive process fails.
        /// </summary>
        public event EventHandler<SocketEventArgs> ReceiveFailed;

        /// <summary>
        /// Fires when a send is completed successfully.
        /// </summary>
        public event EventHandler<EventArgs> SendCompleted;

        /// <summary>
        /// Fires when a send fails to start or complete.
        /// </summary>
        public event EventHandler<SocketEventArgs> SendFailed;

/*/ Methods /*/

    // Public

        /// <summary>
        /// Initiates the connection process to the remote endpoint.
        /// </summary>
        /// <param name="host">Host to connect to.</param>
        /// <param name="port">Port to connect to.</param>
        public void Connect(string host, int port)
        {
            this.host = host;
            this.port = port;

            this.remoteEndPoint = new DnsEndPoint(host, port);
            this.argsConnect.RemoteEndPoint = this.remoteEndPoint;
            this.argsReceive.RemoteEndPoint = this.remoteEndPoint;

            foreach (var sae in this.argsSend)
            {
                sae.RemoteEndPoint = this.remoteEndPoint;
            }

            this.Connect();
        }

        /// <summary>
        /// Disposes of any resources being used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(true);
        }

        /// <summary>
        /// Send text to the remote endpoint.
        /// </summary>
        /// <param name="text">Text to send.</param>
        public void Send(string text)
        {
            this.Send(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Send bytes to the remote endpoint.
        /// </summary>
        /// <param name="bytes">Bytes to send.</param>
        public void Send(byte[] bytes)
        {
            var sendArgs = this.GetSendArgFromPool();

            sendArgs.SetBuffer(bytes, 0, bytes.Length);

            if (!this.socket.SendAsync(sendArgs))
            {
                this.SocketSend_Completed(this, sendArgs);
            }
        }

    // Protected

        /// <summary>
        /// Disposes of any resources being used by this instance.
        /// </summary>
        /// <param name="disposing">Was dispose explicitly called.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.socket?.Dispose();
            }

            this.disposed = true;
        }

    // Private
        private void Connect()
        {
            if (!this.socket.ConnectAsync(this.argsConnect))
            {
                this.SocketConnect_Completed(this, this.argsConnect);
            }
        }

        private SocketAsyncEventArgs GetSendArgFromPool()
        {
            var result = this.argsSend.First(x => x.UserToken == null);
            result.UserToken = DateTime.UtcNow;
            return result;
        }

        private void Receive()
        {
            this.argsReceive.SetBuffer(0, DefaultBufferSize);

            if (!this.socket.ReceiveAsync(this.argsReceive))
            {
                this.SocketReceive_Completed(this, this.argsReceive);
            }
        }

        private void ReturnSendArgToPool(SocketAsyncEventArgs args)
        {
            args.UserToken = null;
        }

        private void SocketConnect_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                this.ConnectCompleted?.Invoke(this, EventArgs.Empty);
                this.Receive();
            }
            else
            {
                this.ConnectFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SocketReceive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    Debug.WriteLine($"[SocketClient] Received {e.BytesTransferred} bytes...");
                    var data = new byte[e.BytesTransferred];
                    Array.Copy(e.Buffer, data, e.BytesTransferred);
                    this.ReceiveCompleted?.Invoke(this, new ReceiveCompletedEventArgs(e.BytesTransferred, data));

                    this.Receive();
                }
                else
                {
                    this.Disconnected?.Invoke(this, EventArgs.Empty);
                    this.socket.Shutdown(SocketShutdown.Both);
                }
            }
            else
            {
                this.ReceiveFailed?.Invoke(this, new SocketEventArgs(e.SocketError, e.SocketError.ToString()));
            }
        }

        private void SocketSend_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Debug.WriteLine($"[SocketClient] Send completed: {Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred)}");
                this.SendCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                this.SendFailed?.Invoke(this, new SocketEventArgs(e.SocketError, e.SocketError.ToString()));
            }

            this.ReturnSendArgToPool(e);
        }
    }
}
