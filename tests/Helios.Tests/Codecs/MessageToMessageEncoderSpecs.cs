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
        private class ExceptionEncoder : MessageToMessageEncoder<object>
        {
            protected override void Encode(IChannelHandlerContext context, object cast, IList<object> output)
            {
                throw new ApplicationException();
            }
        }

        [Fact]
        public void Should_propagate_exceptions()
        {
            var ec = new EmbeddedChannel(new ExceptionEncoder());
            var agg = Assert.Throws<AggregateException>(() =>
            {
                ec.WriteOutbound(new object());
            });
            Assert.IsType<EncoderException>(agg.InnerExceptions[0]);
        }
    }
}
