using System.Threading;
using Helios.Channels;

namespace Helios.Tests.Channels
{
    public class ReadCountAwaiter : ChannelHandlerAdapter
    {
        private readonly ManualResetEventSlim _resetEvent;
        private readonly int _expectedReadCount;
        private int _actualReadCount;

        public ReadCountAwaiter(ManualResetEventSlim resetEvent, int expectedReadCount)
        {
            _resetEvent = resetEvent;
            _expectedReadCount = expectedReadCount;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if(++_actualReadCount == _expectedReadCount)
                _resetEvent.Set();
            context.FireChannelRead(message);
        }
    }
}