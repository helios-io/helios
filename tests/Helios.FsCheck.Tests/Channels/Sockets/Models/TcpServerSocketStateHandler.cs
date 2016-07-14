// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using System.Threading.Tasks;
using Helios.Channels;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public class TcpServerSocketStateHandler : ChannelHandlerAdapter
    {
        public TcpServerSocketStateHandler(ITcpServerSocketModel state)
        {
            State = state;
        }

        public ITcpServerSocketModel State { get; private set; }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            State = State.AddLocalChannel(context.Channel).AddClient((IPEndPoint) context.Channel.RemoteAddress);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            State = State.RemoveLocalChannel(context.Channel).RemoveClient((IPEndPoint) context.Channel.RemoteAddress);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is int)
            {
                State = State.WriteMessages((int) message);
            }
            return base.WriteAsync(context, message);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is int)
            {
                var i = (int) message;
                State = State.ReceiveMessages(i);

                // echo the reply back to the client
                context.WriteAndFlushAsync(i);
            }
        }
    }
}