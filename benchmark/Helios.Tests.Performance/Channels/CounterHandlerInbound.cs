// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Channels;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    internal class CounterHandlerInbound : ChannelHandlerAdapter
    {
        private readonly Counter _throughput;

        public CounterHandlerInbound(Counter throughput)
        {
            _throughput = throughput;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            _throughput.Increment();
            context.FireChannelRead(message);
        }
    }
}

