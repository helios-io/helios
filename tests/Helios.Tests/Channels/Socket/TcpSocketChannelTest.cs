// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Sockets;
using Helios.Codecs;
using Helios.Logging;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Channels.Socket
{
    public class TcpSocketChannelTest
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<TcpSocketChannelTest>();
        private static readonly IPEndPoint TEST_ADDRESS = new IPEndPoint(IPAddress.IPv6Loopback, 0);

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

                var address = (IPEndPoint) sb.BindAsync(IPAddress.IPv6Loopback, 0).Result.LocalAddress;
                var s = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(address.Address, address.Port);

                var inputStream = new NetworkStream(s, true);
                var buf = new byte[8192];
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


        [Fact]
        public void TcpSocketChannel_can_connect_to_TcpServerSocketChannel()
        {
            IEventLoopGroup group1 = new MultithreadEventLoopGroup(2);
            IEventLoopGroup group2 = new MultithreadEventLoopGroup(2);

            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var reads = 100;
            var resetEvent = new ManualResetEventSlim();

            cb.Group(group1)
                .Channel<TcpSocketChannel>()
                .Handler(
                    new ActionChannelInitializer<TcpSocketChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4))
                                .AddLast(new LengthFieldPrepender(4, false))
                                .AddLast(new IntCodec())
                                .AddLast(new TestHandler());
                        }));

            sb.Group(group2)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(
                    new ActionChannelInitializer<TcpSocketChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4))
                                .AddLast(new LengthFieldPrepender(4, false))
                                .AddLast(new IntCodec())
                                .AddLast(new ReadCountAwaiter(resetEvent, reads))
                                .AddLast(new TestHandler());
                        }));

            IChannel sc = null;
            IChannel cc = null;
            try
            {
                // Start server
                sc = sb.BindAsync(TEST_ADDRESS).Result;

                // Connect to the server
                cc = cb.ConnectAsync(sc.LocalAddress).Result;

                foreach (var read in Enumerable.Range(0, reads))
                {
                    cc.WriteAndFlushAsync(read);
                }
                Assert.True(resetEvent.Wait(15000));
            }
            finally
            {
                CloseChannel(cc);
                CloseChannel(sc);
                Task.WaitAll(group1.ShutdownGracefullyAsync(), group2.ShutdownGracefullyAsync());
            }
        }

        private static void CloseChannel(IChannel cc)
        {
            cc?.CloseAsync().Wait();
        }

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
                _tasks.Enqueue(
                    context.WriteAsync(context.Allocator.Buffer().WriteZero(1048576))
                        .ContinueWith(tr => channel.CloseAsync(),
                            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion)
                        .Unwrap());
                _tasks.Enqueue(context.WriteAsync(context.Allocator.Buffer().WriteZero(1048576)));
                context.Flush();
                _tasks.Enqueue(context.WriteAsync(context.Allocator.Buffer().WriteZero(1048576)));
                context.Flush();
            }
        }

        private class TestHandler : ChannelHandlerAdapter
        {
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                Logger.Info("Received message: {0}", message);
                ReferenceCountUtil.SafeRelease(message);
            }
        }
    }
}

