// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading.Tasks;
using Helios.Channels;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    internal class CounterHandlerOutbound : ChannelHandlerAdapter
    {
        private readonly Counter _throughput;

        public CounterHandlerOutbound(Counter throughput)
        {
            _throughput = throughput;
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            _throughput.Increment();
            return context.WriteAsync(message);
        }
    }
}