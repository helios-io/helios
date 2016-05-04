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
