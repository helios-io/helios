// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Channels;

namespace Helios.Tests.Performance.Channels
{
    public class ReadFinishedHandler : ChannelHandlerAdapter
    {
        private readonly int _expectedReads;
        private readonly IReadFinishedSignal _signal;
        private int _actualReads;

        public ReadFinishedHandler(IReadFinishedSignal signal, int expectedReads)
        {
            _signal = signal;
            _expectedReads = expectedReads;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (++_actualReads == _expectedReads)
            {
                _signal.Signal();
            }
            context.FireChannelRead(message);
        }
    }
}

