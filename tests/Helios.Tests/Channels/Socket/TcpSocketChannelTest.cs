using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Channels;

namespace Helios.Tests.Channels.Socket
{
    public class TcpSocketChannelTest
    {
        private class ChannelFlushCloseHandler : ChannelHandlerAdapter
        {
            private readonly ConcurrentQueue<Task> _tasks;

            public ChannelFlushCloseHandler(ConcurrentQueue<Task> tasks)
            {
                _tasks = tasks;
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
                
            }
        }

        public void TcpSocketChannel_Flush_should_not_be_reentrant_after_Close()
        {
            
        }
    }
}
