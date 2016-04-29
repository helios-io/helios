using System;
using System.Diagnostics.Contracts;
using System.Net.Sockets;

namespace Helios.Channels.Sockets
{
    /// <summary>
    /// Default <see cref="IChannelConfiguration"/> for <see cref="ISocketChannel"/>
    /// </summary>
    public class DefaultSocketChannelConfiguration : DefaultChannelConfiguration, ISocketChannelConfig
    {
        protected readonly Socket Socket;
        private volatile bool _allowHalfClosure;

        public DefaultSocketChannelConfiguration(ISocketChannel channel, Socket socket) : base(channel)
        {
            Contract.Requires(socket != null);
            Socket = socket;

            // Enable TCP_NODELAY by default if possible.
            try
            {
                TcpNoDelay = true;
            }
            catch
            {
            }
        }

        public override T GetOption<T>(ChannelOption<T> option)
        {
            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                return (T) (object) this.ReceiveBufferSize;
            }
            if (ChannelOption.SoSndbuf.Equals(option))
            {
                return (T) (object) this.SendBufferSize;
            }
            if (ChannelOption.TcpNodelay.Equals(option))
            {
                return (T) (object) this.TcpNoDelay;
            }
            if (ChannelOption.SoKeepalive.Equals(option))
            {
                return (T) (object) this.KeepAlive;
            }
            if (ChannelOption.SoReuseaddr.Equals(option))
            {
                return (T) (object) this.ReuseAddress;
            }
            if (ChannelOption.SoLinger.Equals(option))
            {
                return (T) (object) this.Linger;
            }
            if (ChannelOption.AllowHalfClosure.Equals(option))
            {
                return (T) (object) this.AllowHalfClosure;
            }

            return base.GetOption(option);
        }

        public override bool SetOption<T>(ChannelOption<T> option, T value)
        {
            if (base.SetOption(option, value))
            {
                return true;
            }

            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                this.ReceiveBufferSize = (int) (object) value;
            }
            else if (ChannelOption.SoSndbuf.Equals(option))
            {
                this.SendBufferSize = (int) (object) value;
            }
            else if (ChannelOption.TcpNodelay.Equals(option))
            {
                this.TcpNoDelay = (bool) (object) value;
            }
            else if (ChannelOption.SoKeepalive.Equals(option))
            {
                this.KeepAlive = (bool) (object) value;
            }
            else if (ChannelOption.SoReuseaddr.Equals(option))
            {
                this.ReuseAddress = (bool) (object) value;
            }
            else if (ChannelOption.SoLinger.Equals(option))
            {
                this.Linger = (int) (object) value;
            }
            else if (ChannelOption.AllowHalfClosure.Equals(option))
            {
                _allowHalfClosure = (bool) (object) value;
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool AllowHalfClosure
        {
            get { return _allowHalfClosure; }
            set { _allowHalfClosure = value; }
        }

        public int ReceiveBufferSize
        {
            get
            {
                try
                {
                    return this.Socket.ReceiveBufferSize;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    this.Socket.ReceiveBufferSize = value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public int SendBufferSize
        {
            get
            {
                try
                {
                    return this.Socket.SendBufferSize;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    this.Socket.SendBufferSize = value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public int Linger
        {
            get
            {
                try
                {
                    LingerOption lingerState = this.Socket.LingerState;
                    return lingerState.Enabled ? lingerState.LingerTime : -1;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    if (value < 0)
                    {
                        this.Socket.LingerState = new LingerOption(false, 0);
                    }
                    else
                    {
                        this.Socket.LingerState = new LingerOption(true, value);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public bool KeepAlive
        {
            get
            {
                try
                {
                    return (int) this.Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive) != 0;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value ? 1 : 0);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public bool ReuseAddress
        {
            get
            {
                try
                {
                    return (int) this.Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress) !=
                           0;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value ? 1 : 0);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }

        public bool TcpNoDelay
        {
            get
            {
                try
                {
                    return this.Socket.NoDelay;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
            set
            {
                try
                {
                    this.Socket.NoDelay = value;
                }
                catch (ObjectDisposedException ex)
                {
                    throw new ChannelException(ex);
                }
                catch (SocketException ex)
                {
                    throw new ChannelException(ex);
                }
            }
        }
    }
}