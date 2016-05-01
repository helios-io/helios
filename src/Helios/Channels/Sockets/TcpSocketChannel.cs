using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Concurrency;

namespace Helios.Channels.Sockets
{
    public class TcpSocketChannel : AbstractSocketByteChannel
    {
        readonly ISocketChannelConfiguration _config;

        /// <summary>
        ///  Create a new instance
        /// </summary>
        public TcpSocketChannel()
            : this(new Socket(SocketType.Stream, ProtocolType.Tcp))
        {
        }

        /// <summary>
        ///  Create a new instance
        /// </summary>
        public TcpSocketChannel(AddressFamily addressFamily)
            : this(new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp))
        {
        }

        /// <summary>
        ///  Create a new instance using the given {@link SocketChannel}.
        /// </summary>
        public TcpSocketChannel(Socket socket)
            : this(null, socket)
        {
        }

        /// <summary>
        ///  Create a new instance
        /// 
        ///  @param parent    the {@link Channel} which created this instance or {@code null} if it was created by the user
        ///  @param socket    the {@link SocketChannel} which will be used
        /// </summary>
        public TcpSocketChannel(IChannel parent, Socket socket)
            : this(parent, socket, false)
        {
        }

        internal TcpSocketChannel(IChannel parent, Socket socket, bool connected)
            : base(parent, socket)
        {
            this._config = new TcpSocketChannelConfig(this, socket);
            if (connected)
            {
                this.OnConnected();
            }
        }

        public override bool DisconnectSupported { get { return false; } }
        protected override EndPoint LocalAddressInternal { get { return Socket.LocalEndPoint; } }
        protected override EndPoint RemoteAddressInternal { get { return Socket.RemoteEndPoint; } }

        public bool IsOutputShutdown
        {
            get { throw new NotImplementedException(); } // todo: impl with stateflags
        }

        public override IChannelConfiguration Configuration { get { return _config; } }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new TcpSocketChannelUnsafe(this);
        }

        public Task ShutdownOutputAsync()
        {
            var tcs = new TaskCompletionSource();
            IEventLoop loop = this.EventLoop;
            if (loop.InEventLoop)
            {
                this.ShutdownOutput0(tcs);
            }
            else
            {
                loop.Execute(promise => this.ShutdownOutput0((TaskCompletionSource)promise), tcs);
            }
            return tcs.Task;
        }

        void ShutdownOutput0(TaskCompletionSource promise)
        {
            try
            {
                this.Socket.Shutdown(SocketShutdown.Send);
                promise.Complete();
            }
            catch (Exception ex)
            {
                promise.SetException(ex);
            }
        }

        protected override bool DoConnect(EndPoint remoteAddress, EndPoint localAddress)
        {
            if (localAddress != null)
            {
                this.Socket.Bind(localAddress);
            }

            bool success = false;
            try
            {
                var eventPayload = new SocketChannelAsyncOperation(this, false);
                eventPayload.RemoteEndPoint = remoteAddress;
                bool connected = !this.Socket.ConnectAsync(eventPayload);
                success = true;
                return connected;
            }
            finally
            {
                if (!success)
                {
                    this.DoClose();
                }
            }
        }

        protected override void DoFinishConnect(SocketChannelAsyncOperation operation)
        {
            try
            {
                operation.Validate();
            }
            finally
            {
                operation.Dispose();
            }
            this.OnConnected();
        }

        void OnConnected()
        {
            this.SetState(StateFlags.Active);

            // preserve local and remote addresses for later availability even if Socket fails
            this.CacheLocalAddress();
            this.CacheRemoteAddress();
        }

        protected override void DoBind(EndPoint localAddress)
        {
            Socket.Bind(localAddress);
        }

        protected override void DoDisconnect()
        {
            this.DoClose();
        }

        protected override void DoClose()
        {
            base.DoClose();
            if (this.ResetState(StateFlags.Open | StateFlags.Active))
            {
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Close(0);
            }
        }

        protected override int DoReadBytes(IByteBuf buf)
        {
            if (!buf.HasArray)
            {
                throw new NotImplementedException("Only IByteBuffer implementations backed by array are supported.");
            }

            SocketError errorCode;
            int received = this.Socket.Receive(buf.Array, buf.ArrayOffset + buf.WriterIndex, buf.WritableBytes, SocketFlags.None, out errorCode);

            switch (errorCode)
            {
                case SocketError.Success:
                    if (received == 0)
                    {
                        return -1; // indicate that socket was closed
                    }
                    break;
                case SocketError.WouldBlock:
                    if (received == 0)
                    {
                        return 0;
                    }
                    break;
                default:
                    throw new SocketException((int)errorCode);
            }

            buf.SetWriterIndex(buf.WriterIndex + received);

            return received;
        }

        protected override int DoWriteBytes(IByteBuf buf)
        {
            if (!buf.HasArray)
            {
                throw new NotImplementedException("Only IByteBuffer implementations backed by array are supported.");
            }

            SocketError errorCode;
            int sent = this.Socket.Send(buf.Array, buf.ArrayOffset + buf.ReaderIndex, buf.ReadableBytes, SocketFlags.None, out errorCode);

            if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock)
            {
                throw new SocketException((int)errorCode);
            }

            if (sent > 0)
            {
                buf.SetReaderIndex(buf.ReaderIndex + sent);
            }

            return sent;
        }

        sealed class TcpSocketChannelUnsafe : SocketByteChannelUnsafe
        {
            public TcpSocketChannelUnsafe(TcpSocketChannel channel)
                : base(channel)
            {
            }
        }

        sealed class TcpSocketChannelConfig : DefaultSocketChannelConfiguration
        {
            public TcpSocketChannelConfig(TcpSocketChannel channel, Socket javaSocket)
                : base(channel, javaSocket)
            {
            }

            protected override void AutoReadCleared()
            {
                ((TcpSocketChannel)this.Channel).ReadPending = false;
            }
        }
    }
}