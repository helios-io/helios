// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Channels;
using NBench;

namespace Helios.HorizontalScaling.Tests.Performance.Channels
{
    public class ErrorCounterHandler : ChannelHandlerAdapter
    {
        private readonly Counter _errorCount;

        public ErrorCounterHandler(Counter errorCount)
        {
            _errorCount = errorCount;
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _errorCount.Increment();
        }
    }
}