// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Logging;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public sealed class TcpClientSocketStateHandler : ChannelHandlerAdapter
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<TcpServerSocketStateHandler>();

        public TcpClientSocketStateHandler() : this(new TcpClientSocketModel())
        {
        }

        public TcpClientSocketStateHandler(TcpClientSocketModel state)
        {
            State = state;
        }

        public TcpClientSocketModel State { get; private set; }

        public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress, EndPoint localAddress)
        {
            State = State.SetChannel(context.Channel).SetState(ConnectionState.Connecting);
            return base.ConnectAsync(context, remoteAddress, localAddress);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            State = State.SetChannel(context.Channel).SetState(ConnectionState.Active);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            State = State.SetState(ConnectionState.Shutdown);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is int)
            {
                State = State.WriteMessages((int) message);
                Logger.Debug("[Client-Write] Writing: {0}", message);
            }
            return context.WriteAsync(message);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is int)
            {
                State = State.ReceiveMessages((int) message);
                Logger.Debug("[Client-Read] Received: {0}", message);
            }
        }
    }
}

