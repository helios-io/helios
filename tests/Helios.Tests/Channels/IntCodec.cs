// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Util;

namespace Helios.Tests.Channels
{
    public class IntCodec : ChannelHandlerAdapter
    {
        public IntCodec(bool releaseMessages = false)
        {
            ReleaseMessages = releaseMessages;
        }

        public bool ReleaseMessages { get; }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuf)
            {
                var buf = (IByteBuf) message;
                var integer = buf.ReadInt();
                if (ReleaseMessages)
                    ReferenceCountUtil.SafeRelease(message);
                context.FireChannelRead(integer);
            }
            else
            {
                context.FireChannelRead(message);
            }
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            if (message is int)
            {
                var buf = Unpooled.Buffer(4).WriteInt((int) message);
                return context.WriteAsync(buf);
            }
            return context.WriteAsync(message);
        }
    }
}