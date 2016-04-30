using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Xunit;

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
                // write a large enough blob of data that it has to be split into multiple writes
                var channel = context.Channel;
                _tasks.Enqueue(context.WriteAsync(context.Allocator.Buffer().WriteZero(1048576)).ContinueWith(tr => channel.CloseAsync(), TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap());
                _tasks.Enqueue(context.WriteAsync(context.Allocator.Buffer().WriteZero(1048576)));
                context.Flush();
                _tasks.Enqueue(context.WriteAsync(context.Allocator.Buffer().WriteZero(1048576)));
                context.Flush();
            }
        }

        [Fact]
        public void TcpSocketChannel_Flush_should_not_be_reentrant_after_Close()
        {
            var eventLoopGroup = new MultithreadEventLoopGroup(1);
            try
            {
                var futures = new ConcurrentQueue<Task>();
                var sb = new ServerBootstrap();
                sb.Group(eventLoopGroup).Channel<TcpServerSocketChannel>().ChildOption(ChannelOption.SoSndbuf, 1024)
                    .ChildHandler(new ChannelFlushCloseHandler(futures));

                var address = (IPEndPoint)sb.BindAsync(IPAddress.IPv6Loopback, 0).Result.LocalAddress;
                var s = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6,SocketType.Stream, ProtocolType.Tcp);
                s.Connect(address.Address, address.Port);

                var inputStream = new NetworkStream(s, true);
                byte[] buf = new byte[8192];
                while (true)
                {
                    var readBytes = inputStream.Read(buf, 0, 8192);
                    if (readBytes == 0)
                    {
                        break;
                    }

                    // Wait a little bit so that the write attempts are split into multiple flush attempts.
                    Thread.Sleep(10);
                }

                s.Close();

                Assert.Equal(3, futures.Count);
                Task future1, future2, future3;
                futures.TryDequeue(out future1);
                futures.TryDequeue(out future2);
                futures.TryDequeue(out future3);
                Assert.True(future1.IsCompleted);
                Assert.False(future1.IsFaulted || future1.IsCanceled);
                Assert.True(future2.IsFaulted || future2.IsCanceled);
                Assert.IsType<ClosedChannelException>(future2.Exception.InnerException);
                Assert.True(future3.IsFaulted || future3.IsCanceled);
                Assert.IsType<ClosedChannelException>(future3.Exception.InnerException);
            }
            finally
            {
                eventLoopGroup.ShutdownGracefullyAsync();
            }
        }
    }
}
