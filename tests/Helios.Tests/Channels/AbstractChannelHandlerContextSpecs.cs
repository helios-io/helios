using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Channels.Embedded;
using Xunit;

namespace Helios.Tests.Channels
{
    public class AbstractChannelHandlerContextSpecs
    {
        /// <summary>
        /// Un-initialized, default channel
        /// </summary>
        private EmbeddedChannel _channel;

        public AbstractChannelHandlerContextSpecs()
        {
            _channel = new EmbeddedChannel(new ActionChannelInitializer<IChannel>(ch => {  }));
        }

        [InlineData(AbstractChannelHandlerContext.MASK_BIND, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_FLUSH, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CONNECT, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_DISCONNECT, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CLOSE, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_DEREGISTER, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_READ, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_WRITE, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASKGROUP_OUTBOUND, typeof(DefaultChannelPipeline.HeadContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CHANNEL_ACTIVE, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CHANNEL_INACTIVE, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CHANNEL_REGISTERED, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CHANNEL_UNREGISTERED, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_EXCEPTION_CAUGHT, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CHANNEL_READ, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_CHANNEL_READ_COMPLETE, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASK_USER_EVENT_TRIGGERED, typeof(DefaultChannelPipeline.TailContext))]
        [InlineData(AbstractChannelHandlerContext.MASKGROUP_INBOUND, typeof(DefaultChannelPipeline.TailContext))]
        [Theory]
        public void DefaultContext_should_support(int methodMask, Type handlerType)
        {
            var actualFlags = AbstractChannelHandlerContext.CalculateSkipPropagationFlags(handlerType);
            Assert.True((actualFlags & methodMask) == 0);
        }
    }
}
