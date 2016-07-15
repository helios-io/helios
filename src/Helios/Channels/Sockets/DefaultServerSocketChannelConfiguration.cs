// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;
using System.Net.Sockets;

namespace Helios.Channels.Sockets
{
    /// <summary>
    ///     The default {@link ServerSocketChannelConfig} implementation.
    /// </summary>
    public class DefaultServerSocketChannelConfig : DefaultChannelConfiguration, IServerSocketChannelConfiguration
    {
        protected readonly Socket Socket;
        private volatile int backlog = 200; //todo: NetUtil.SOMAXCONN;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DefaultServerSocketChannelConfig(IServerSocketChannel channel, Socket socket)
            : base(channel)
        {
            Contract.Requires(socket != null);

            Socket = socket;
        }

        public bool ReuseAddress
        {
            get
            {
                try
                {
                    return (int) Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress) != 0;
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

        public override T GetOption<T>(ChannelOption<T> option)
        {
            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                return (T) (object) ReceiveBufferSize;
            }
            if (ChannelOption.SoReuseaddr.Equals(option))
            {
                return (T) (object) ReuseAddress;
            }
            if (ChannelOption.SoBacklog.Equals(option))
            {
                return (T) (object) Backlog;
            }

            return base.GetOption(option);
        }

        public override bool SetOption<T>(ChannelOption<T> option, T value)
        {
            Validate(option, value);

            if (ChannelOption.SoRcvbuf.Equals(option))
            {
                ReceiveBufferSize = (int) (object) value;
            }
            else if (ChannelOption.SoReuseaddr.Equals(option))
            {
                ReuseAddress = (bool) (object) value;
            }
            else if (ChannelOption.SoBacklog.Equals(option))
            {
                Backlog = (int) (object) value;
            }
            else
            {
                return base.SetOption(option, value);
            }

            return true;
        }

        public int Backlog
        {
            get { return backlog; }
            set
            {
                Contract.Requires(value >= 0);

                backlog = value;
            }
        }
    }
}