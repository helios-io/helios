// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using Helios.Channels;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Xunit;

namespace Helios.Tests.Codecs
{
    public class MessageToMessageEncoderSpecs
    {
        [Fact]
        public void Should_propagate_exceptions()
        {
            var ec = new EmbeddedChannel(new ExceptionEncoder());
            var agg = Assert.Throws<AggregateException>(() => { ec.WriteOutbound(new object()); });
            Assert.IsType<EncoderException>(agg.InnerExceptions[0]);
        }

        private class ExceptionEncoder : MessageToMessageEncoder<object>
        {
            protected override void Encode(IChannelHandlerContext context, object cast, List<object> output)
            {
                throw new ApplicationException();
            }
        }
    }
}

