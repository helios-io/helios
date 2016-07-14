// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;
using Helios.Channels;

namespace Helios.Tests.Channels
{
    public class ReadCountAwaiter : ChannelHandlerAdapter
    {
        private readonly int _expectedReadCount;
        private readonly ManualResetEventSlim _resetEvent;
        private int _actualReadCount;

        public ReadCountAwaiter(ManualResetEventSlim resetEvent, int expectedReadCount)
        {
            _resetEvent = resetEvent;
            _expectedReadCount = expectedReadCount;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (++_actualReadCount == _expectedReadCount)
                _resetEvent.Set();
            context.FireChannelRead(message);
        }
    }
}