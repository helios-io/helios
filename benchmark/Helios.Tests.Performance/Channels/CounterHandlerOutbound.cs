using System.Threading.Tasks;
using Helios.Channels;
using Helios.Concurrency;
using NBench;

namespace Helios.Tests.Performance.Channels
{
    class CounterHandlerOutbound : ChannelHandlerAdapter
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