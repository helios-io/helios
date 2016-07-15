// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using System.Net.Sockets;

namespace Helios.Channels.Sockets
{
    /// <summary>
    ///     Default <see cref="IChannelConfiguration" /> for <see cref="ISocketChannel" />
    /// </summary>
    public class DefaultSocketChannelConfiguration : DefaultChannelConfiguration, ISocketChannelConfiguration
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
                return (T) (object) ReceiveBufferSize;
            }
            if (ChannelOption.SoSndbuf.Equals(option))
            {
                return (T) (object) SendBufferSize;
            }
            if (ChannelOption.TcpNodelay.Equals(option))
            {
                return (T) (object) TcpNoDelay;
            }
            if (ChannelOption.SoKeepalive.Equals(option))
            {
                return (T) (object) KeepAlive;
            }
            if (ChannelOption.SoReuseaddr.Equals(option))
            {
                return (T) (object) ReuseAddress;
            }
            if (ChannelOption.SoLinger.Equals(option))
            {
                return (T) (object) Linger;
            }
            if (ChannelOption.AllowHalfClosure.Equals(option))
            {
                return (T) (object) AllowHalfClosure;
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
                ReceiveBufferSize = (int) (object) value;
            }
            else if (ChannelOption.SoSndbuf.Equals(option))
            {
                SendBufferSize = (int) (object) value;
            }
            else if (ChannelOption.TcpNodelay.Equals(option))
            {
                TcpNoDelay = (bool) (object) value;
            }
            else if (ChannelOption.SoKeepalive.Equals(option))
            {
                KeepAlive = (bool) (object) value;
            }
            else if (ChannelOption.SoReuseaddr.Equals(option))
            {
                ReuseAddress = (bool) (object) value;
            }
            else if (ChannelOption.SoLinger.Equals(option))
            {
                Linger = (int) (object) value;
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
                    return Socket.ReceiveBufferSize;
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
                    Socket.ReceiveBufferSize = value;
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
                    return Socket.SendBufferSize;
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
                    Socket.SendBufferSize = value;
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
                    var lingerState = Socket.LingerState;
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
                        Socket.LingerState = new LingerOption(false, 0);
                    }
                    else
                    {
                        Socket.LingerState = new LingerOption(true, value);
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
                    return (int) Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive) != 0;
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
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, value ? 1 : 0);
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
                    return (int) Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress) !=
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
                    Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value ? 1 : 0);
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
                    return Socket.NoDelay;
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
                    Socket.NoDelay = value;
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