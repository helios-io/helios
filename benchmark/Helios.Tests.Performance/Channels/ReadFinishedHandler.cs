using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Channels;

namespace Helios.Tests.Performance.Channels
{
    public class ReadFinishedHandler : ChannelHandlerAdapter
    {
        private readonly int _expectedReads;
        private int _actualReads;
        private readonly IReadFinishedSignal _signal;

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
