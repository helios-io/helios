using System.Diagnostics;
using System.Threading;
using Helios.Channels;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    class CounterHandlerInbound : ChannelHandlerAdapter
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