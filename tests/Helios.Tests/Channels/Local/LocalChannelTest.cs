// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Channels.Bootstrap;
using Helios.Channels.Local;
using Helios.Codecs;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Channels.Local
{
    public class LocalChannelTest : IDisposable
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<LocalChannelTest>();
        private static readonly LocalAddress TEST_ADDRESS = new LocalAddress("test.id");

        private readonly IEventLoopGroup _group1;
        private readonly IEventLoopGroup _group2;
        private readonly IEventLoopGroup _sharedGroup;

        public LocalChannelTest()
        {
            _group1 = new MultithreadEventLoopGroup(2);
            _group2 = new MultithreadEventLoopGroup(2);
            _sharedGroup = new MultithreadEventLoopGroup(1);
        }

        public void Dispose()
        {
            var t1 = _group1.ShutdownGracefullyAsync();
            var t2 = _group2.ShutdownGracefullyAsync();
            var t3 = _sharedGroup.ShutdownGracefullyAsync();
            //Task.WaitAll(t1, t2, t3);
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_batch_read_should_not_NRE()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var reads = 100;
            var resetEvent = new ManualResetEventSlim();

            cb.Group(_group1)
                .Channel<LocalChannel>()
                .Handler(
                    new ActionChannelInitializer<LocalChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4))
                                .AddLast(new LengthFieldPrepender(4, false))
                                .AddLast(new TestHandler());
                        }));

            sb.Group(_group2)
                .Channel<LocalServerChannel>()
                .ChildHandler(
                    new ActionChannelInitializer<LocalChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4))
                                .AddLast(new LengthFieldPrepender(4, false))
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
                    cc.WriteAndFlushAsync(Unpooled.Buffer(4).WriteInt(read));
                }
                Assert.True(resetEvent.Wait(5000));
            }
            finally
            {
                CloseChannel(sc);
                CloseChannel(cc);
            }
        }

        /// <summary>
        ///     Verify that messages written out to a server following a connection
        ///     are read by a child <see cref="LocalChannel" />
        /// </summary>
        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_writes_to_server_should_be_read_by_LocalChannel()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var reads = new[] {-11, 2, 9, 13, 1013, 1, 4};
            var resetEvent = new ManualResetEventSlim();
            var accumulatedReads = new List<int>();

            cb.Group(_group1)
                .Channel<LocalChannel>()
                .Handler(
                    new ActionChannelInitializer<LocalChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4))
                                .AddLast(new LengthFieldPrepender(4, false))
                                .AddLast(new IntCodec());
                        }));

            sb.Group(_group2)
                .Channel<LocalServerChannel>()
                .ChildHandler(
                    new ActionChannelInitializer<LocalChannel>(
                        channel =>
                        {
                            channel.Pipeline.AddLast(new LengthFieldBasedFrameDecoder(20, 0, 4, 0, 4))
                                .AddLast(new LengthFieldPrepender(4, false))
                                .AddLast(new IntCodec())
                                .AddLast(new ReadAssertHandler(accumulatedReads, resetEvent, reads));
                        }));

            IChannel sc = null;
            IChannel cc = null;

            try
            {
                // Start server
                sc = sb.BindAsync(TEST_ADDRESS).Result;

                // Connect to the server
                cc = cb.ConnectAsync(sc.LocalAddress).Result;

                foreach (var read in reads)
                {
                    cc.WriteAsync(read);
                }
                cc.Flush();
                resetEvent.Wait(200);
                Assert.Equal(reads, accumulatedReads);
            }
            finally
            {
                CloseChannel(sc);
                CloseChannel(cc);
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_should_reuse_LocalAddress()
        {
            for (var i = 0; i < 2; i++)
            {
                var cb = new ClientBootstrap();
                var sb = new ServerBootstrap();

                cb.Group(_group1).Channel<LocalChannel>().Handler(new TestHandler());

                sb.Group(_group2).Channel<LocalServerChannel>().ChildHandler(new ActionChannelInitializer<LocalChannel>(
                    channel => { channel.Pipeline.AddLast(new TestHandler()); }));

                IChannel sc = null;
                IChannel cc = null;

                try
                {
                    // Start server
                    sc = sb.BindAsync(TEST_ADDRESS).Result;
                    var latch = new CountdownEvent(1);

                    // Connect to the server
                    cc = cb.ConnectAsync(sc.LocalAddress).Result;
                    var cCpy = cc;
                    cc.EventLoop.Execute(o =>
                    {
                        var c = (LocalChannel) o;
                        c.Pipeline.FireChannelRead("Hello, World");
                        latch.Signal();
                    }, cCpy);

                    latch.Wait(TimeSpan.FromSeconds(5));
                    Assert.True(latch.IsSet);

                    CloseChannel(cc);
                    CloseChannel(sc);
                    sc.CloseCompletion.Wait();

                    Assert.Null(LocalChannelRegistry.Get(TEST_ADDRESS));
                }
                finally
                {
                    CloseChannel(cc);
                    CloseChannel(sc);
                }
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_WriteAsync_should_fail_fast_on_closed_channel()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();

            cb.Group(_group1).Channel<LocalChannel>().Handler(new TestHandler());

            sb.Group(_group2).Channel<LocalServerChannel>().ChildHandler(new ActionChannelInitializer<LocalChannel>(
                channel => { channel.Pipeline.AddLast(new TestHandler()); }));

            IChannel sc = null;
            IChannel cc = null;

            try
            {
                // Start server
                sc = sb.BindAsync(TEST_ADDRESS).Result;
                var latch = new CountdownEvent(1);

                // Connect to the server
                cc = cb.ConnectAsync(sc.LocalAddress).Result;

                // Close the channel and write something
                cc.CloseAsync().Wait();
                var ag =
                    Assert.Throws<AggregateException>(() => { cc.WriteAndFlushAsync(new object()).Wait(); }).Flatten();
                Assert.IsType<ClosedChannelException>(ag.InnerException);
            }
            finally
            {
                CloseChannel(cc);
                CloseChannel(sc);
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalServerChannel_should_be_able_to_close_channel_on_same_EventLoop()
        {
            var latch = new CountdownEvent(1);
            var sb = new ServerBootstrap();

            sb.Group(_group2).Channel<LocalServerChannel>().ChildHandler(new ReadHandler1(latch));

            IChannel sc = null;
            IChannel cc = null;

            try
            {
                sc = sb.BindAsync(TEST_ADDRESS).Result;

                var b = new ClientBootstrap()
                    .Group(_group2)
                    .Channel<LocalChannel>().Handler(new ReadHandler2());
                cc = b.ConnectAsync(sc.LocalAddress).Result;
                cc.WriteAndFlushAsync(new object());
                Assert.True(latch.Wait(TimeSpan.FromSeconds(5)));
            }
            finally
            {
                CloseChannel(cc);
                CloseChannel(sc);
            }
        }


        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_close_when_WritePromiseComplete_should_still_preserve_order()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var messageLatch = new CountdownEvent(2);
            var data = Unpooled.WrappedBuffer(new byte[1024]);

            try
            {
                cb.Group(_group1)
                    .Channel<LocalChannel>()
                    .Handler(new TestHandler());

                sb.Group(_group2)
                    .Channel<LocalServerChannel>()
                    .ChildHandler(new ReadCountdown1(messageLatch, data));

                IChannel sc = null;
                IChannel cc = null;

                try
                {
                    // Start server
                    sc = sb.BindAsync(TEST_ADDRESS).Result;

                    // Connect to server
                    cc = cb.ConnectAsync(sc.LocalAddress).Result;

                    var ccCpy = cc;

                    // Make sure a write operation is executed in the eventloop
                    cc.Pipeline.LastContext()
                        .Executor.Execute(
                            () =>
                            {
                                ccCpy.WriteAndFlushAsync(data.Duplicate().Retain())
                                    .ContinueWith(tr => { ccCpy.Pipeline.LastContext().CloseAsync(); });
                            });

                    Assert.True(messageLatch.Wait(TimeSpan.FromSeconds(5)));
                    Assert.False(cc.IsOpen);
                }
                finally
                {
                    CloseChannel(sc);
                    CloseChannel(cc);
                }
            }
            finally
            {
                data.Release();
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_write_when_WritePromiseComplete_should_still_preserve_order()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var messageLatch = new CountdownEvent(2);
            var data1 = Unpooled.WrappedBuffer(new byte[1024]);
            var data2 = Unpooled.WrappedBuffer(new byte[512]);

            try
            {
                cb.Group(_group1)
                    .Channel<LocalChannel>()
                    .Handler(new TestHandler());

                sb.Group(_group2)
                    .Channel<LocalServerChannel>()
                    .ChildHandler(new ReadCountdown2(messageLatch, data1, data2));

                IChannel sc = null;
                IChannel cc = null;

                try
                {
                    // Start server
                    sc = sb.BindAsync(TEST_ADDRESS).Result;

                    // Connect to server
                    cc = cb.ConnectAsync(sc.LocalAddress).Result;

                    var ccCpy = cc;

                    // Make sure a write operation is executed in the eventloop
                    cc.Pipeline.LastContext().Executor.Execute(() =>
                    {
                        Logger.Info("Writing message 1");
                        ccCpy.WriteAndFlushAsync(data1.Duplicate().Retain()).ContinueWith(tr =>
                        {
                            Logger.Info("Writing message 2");
                            ccCpy.WriteAndFlushAsync(data2.Duplicate().Retain());
                        });
                    });

                    Assert.True(messageLatch.Wait(TimeSpan.FromSeconds(5)));
                }
                finally
                {
                    CloseChannel(sc);
                    CloseChannel(cc);
                }
            }
            finally
            {
                data1.Release();
                data2.Release();
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_PeerWrite_when_WritePromiseComplete_in_different_eventloop_should_still_preserve_order()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var messageLatch = new CountdownEvent(2);
            var data1 = Unpooled.WrappedBuffer(new byte[1024]);
            var data2 = Unpooled.WrappedBuffer(new byte[512]);
            var serverLatch = new CountdownEvent(1);
            var serverChannelRef = new AtomicReference<IChannel>();

            try
            {
                cb.Group(_group1)
                    .Channel<LocalChannel>()
                    .Handler(new ReadCountdown3(messageLatch, data2));

                sb.Group(_group2)
                    .Channel<LocalServerChannel>()
                    .ChildHandler(new ActionChannelInitializer<LocalChannel>(channel =>
                    {
                        channel.Pipeline.AddLast(new ReadCountdown3(messageLatch, data1));
                        serverChannelRef = channel;
                        serverLatch.Signal();
                    }));

                IChannel sc = null;
                IChannel cc = null;

                try
                {
                    // Start server
                    sc = sb.BindAsync(TEST_ADDRESS).Result;

                    // Connect to server
                    cc = cb.ConnectAsync(sc.LocalAddress).Result;
                    serverLatch.Wait(TimeSpan.FromSeconds(5));
                    Assert.True(serverLatch.IsSet);

                    var ccCpy = cc;

                    // Make sure a write operation is executed in the eventloop
                    cc.Pipeline.LastContext().Executor.Execute(() =>
                    {
                        ccCpy.WriteAndFlushAsync(data1.Duplicate().Retain()).ContinueWith(tr =>
                        {
                            var severChannelCopy = serverChannelRef.Value;
                            severChannelCopy.WriteAndFlushAsync(data2.Duplicate().Retain());
                        });
                    });

                    messageLatch.Wait(TimeSpan.FromSeconds(5));
                    Assert.True(messageLatch.IsSet);
                }
                finally
                {
                    CloseChannel(sc);
                    CloseChannel(cc);
                }
            }
            finally
            {
                data1.Release();
                data2.Release();
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_PeerWrite_when_WritePromiseComplete_in_same_eventloop_should_still_preserve_order()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var messageLatch = new CountdownEvent(2);
            var data1 = Unpooled.WrappedBuffer(new byte[1024]);
            var data2 = Unpooled.WrappedBuffer(new byte[512]);
            var serverLatch = new CountdownEvent(1);
            var serverChannelRef = new AtomicReference<IChannel>();

            try
            {
                cb.Group(_sharedGroup)
                    .Channel<LocalChannel>()
                    .Handler(new ReadCountdown3(messageLatch, data2));

                sb.Group(_sharedGroup)
                    .Channel<LocalServerChannel>()
                    .ChildHandler(new ActionChannelInitializer<LocalChannel>(channel =>
                    {
                        channel.Pipeline.AddLast(new ReadCountdown3(messageLatch, data1));
                        serverChannelRef = channel;
                        serverLatch.Signal();
                    }));

                IChannel sc = null;
                IChannel cc = null;

                try
                {
                    // Start server
                    sc = sb.BindAsync(TEST_ADDRESS).Result;

                    // Connect to server
                    cc = cb.ConnectAsync(sc.LocalAddress).Result;
                    serverLatch.Wait(TimeSpan.FromSeconds(5));
                    Assert.True(serverLatch.IsSet);

                    var ccCpy = cc;

                    // Make sure a write operation is executed in the eventloop
                    cc.Pipeline.LastContext().Executor.Execute(() =>
                    {
                        ccCpy.WriteAndFlushAsync(data1.Duplicate().Retain()).ContinueWith(tr =>
                        {
                            var severChannelCopy = serverChannelRef.Value;
                            severChannelCopy.WriteAndFlushAsync(data2.Duplicate().Retain());
                        });
                    });

                    messageLatch.Wait(TimeSpan.FromSeconds(5));
                    Assert.True(messageLatch.IsSet);
                }
                finally
                {
                    CloseChannel(sc);
                    CloseChannel(cc);
                }
            }
            finally
            {
                data1.Retain();
                data2.Retain();
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_PeerClose_when_WritePromiseComplete_in_same_eventloop_should_still_preserve_order()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();
            var messageLatch = new CountdownEvent(2);
            var data = Unpooled.WrappedBuffer(new byte[1024]);
            var serverLatch = new CountdownEvent(1);
            var serverChannelRef = new AtomicReference<IChannel>();

            try
            {
                cb.Group(_sharedGroup)
                    .Channel<LocalChannel>()
                    .Handler(new TestHandler());

                sb.Group(_sharedGroup)
                    .Channel<LocalServerChannel>()
                    .ChildHandler(new ActionChannelInitializer<LocalChannel>(channel =>
                    {
                        channel.Pipeline.AddLast(new ReadCountdown1(messageLatch, data));
                        serverChannelRef = channel;
                        serverLatch.Signal();
                    }));

                IChannel sc = null;
                IChannel cc = null;

                try
                {
                    // Start server
                    sc = sb.BindAsync(TEST_ADDRESS).Result;

                    // Connect to server
                    cc = cb.ConnectAsync(sc.LocalAddress).Result;
                    serverLatch.Wait(TimeSpan.FromSeconds(5));
                    Assert.True(serverLatch.IsSet);

                    var ccCpy = cc;

                    // Make sure a write operation is executed in the eventloop
                    cc.Pipeline.LastContext().Executor.Execute(() =>
                    {
                        ccCpy.WriteAndFlushAsync(data.Duplicate().Retain()).ContinueWith(tr =>
                        {
                            var severChannelCopy = serverChannelRef.Value;
                            severChannelCopy.CloseAsync();
                        });
                    });

                    Assert.True(messageLatch.Wait(TimeSpan.FromSeconds(5)));
                    Assert.False(cc.IsOpen);
                    Assert.False(serverChannelRef.Value.IsOpen);
                }
                finally
                {
                    CloseChannel(sc);
                    CloseChannel(cc);
                }
            }
            finally
            {
                data.Release();
            }
        }

        [Fact(Skip = "Not relevant for Helios 2.0")]
        public void LocalChannel_should_not_fire_channel_active_before_connecting()
        {
            var cb = new ClientBootstrap();
            var sb = new ServerBootstrap();

            cb.Group(_group1).Channel<LocalChannel>().Handler(new DummyHandler());

            sb.Group(_group2).Channel<LocalServerChannel>().ChildHandler(new ActionChannelInitializer<LocalChannel>(
                channel => { channel.Pipeline.AddLast(new TestHandler()); }));

            IChannel sc = null;
            IChannel cc = null;

            try
            {
                // Start server
                sc = sb.BindAsync(TEST_ADDRESS).Result;

                cc = cb.Register().Result;

                var promise = new TaskCompletionSource();
                var assertPromise = new TaskCompletionSource();

                cc.Pipeline.AddLast(new PromiseAssertHandler(promise, assertPromise));

                // Connect to the server
                cc.ConnectAsync(sc.LocalAddress).Wait();

                assertPromise.Task.Wait();
                Assert.True(promise.Task.IsCompleted);
            }
            finally
            {
                CloseChannel(cc);
                CloseChannel(sc);
            }
        }

        private static void CloseChannel(IChannel cc)
        {
            cc?.CloseAsync().Wait();
        }

        private class ReadAssertHandler : ChannelHandlerAdapter
        {
            private readonly List<int> _accumulatedReads;
            private readonly Queue<int> _expectedReads;
            private readonly ManualResetEventSlim _resetEventSlim;

            public ReadAssertHandler(List<int> accumulatedReads, ManualResetEventSlim resetEventSlim,
                params int[] expectedReads)
            {
                _accumulatedReads = accumulatedReads;
                _resetEventSlim = resetEventSlim;
                _expectedReads = new Queue<int>(expectedReads);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var next = _expectedReads.Dequeue();
                Assert.Equal(next, message);
                _accumulatedReads.Add((int) message);
                if (!_expectedReads.Any())
                {
                    _resetEventSlim.Set();
                }
            }
        }

        private class ReadHandler1 : ChannelHandlerAdapter
        {
            private readonly CountdownEvent _event;

            public ReadHandler1(CountdownEvent @event)
            {
                _event = @event;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                _event.Signal();
                context.CloseAsync();
            }
        }

        private class ReadHandler2 : ChannelHandlerAdapter
        {
            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                //discard
            }
        }

        private class ReadCountdown1 : ChannelHandlerAdapter
        {
            private readonly IByteBuf _expectedData;
            private readonly CountdownEvent _latch;

            public ReadCountdown1(CountdownEvent latch, IByteBuf expectedData)
            {
                _latch = latch;
                _expectedData = expectedData;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                if (AbstractByteBuf.ByteBufComparer.Equals(_expectedData, message as IByteBuf))
                {
                    ReferenceCountUtil.SafeRelease(message);
                    _latch.Signal();
                }
                else
                {
                    base.ChannelRead(context, message);
                }
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                _latch.Signal();
                base.ChannelInactive(context);
            }
        }

        private class ReadCountdown2 : ChannelHandlerAdapter
        {
            private readonly IByteBuf _expectedData1;
            private readonly IByteBuf _expectedData2;
            private readonly CountdownEvent _latch;

            public ReadCountdown2(CountdownEvent latch, IByteBuf expectedData1, IByteBuf expectedData2)
            {
                _latch = latch;
                _expectedData1 = expectedData1;
                _expectedData2 = expectedData2;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var count = _latch.CurrentCount;
                if ((AbstractByteBuf.ByteBufComparer.Equals(_expectedData1, message as IByteBuf) && count == 2)
                    || (AbstractByteBuf.ByteBufComparer.Equals(_expectedData2, message as IByteBuf) && count == 1))
                {
                    Logger.Info("Received message");
                    ReferenceCountUtil.SafeRelease(message);
                    _latch.Signal();
                }
                else
                {
                    base.ChannelRead(context, message);
                }
            }
        }

        private class ReadCountdown3 : ChannelHandlerAdapter
        {
            private readonly IByteBuf _expectedData;
            private readonly CountdownEvent _latch;

            public ReadCountdown3(CountdownEvent latch, IByteBuf expectedData)
            {
                _latch = latch;
                _expectedData = expectedData;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                if (AbstractByteBuf.ByteBufComparer.Equals(_expectedData, message as IByteBuf))
                {
                    ReferenceCountUtil.SafeRelease(message);
                    _latch.Signal();
                }
                else
                {
                    base.ChannelRead(context, message);
                }
            }
        }

        private class DummyHandler : ChannelHandlerAdapter
        {
        }

        private class PromiseAssertHandler : ChannelHandlerAdapter
        {
            private readonly TaskCompletionSource _assertPromise;
            private readonly TaskCompletionSource _promise;

            public PromiseAssertHandler(TaskCompletionSource promise, TaskCompletionSource assertPromise)
            {
                _promise = promise;
                _assertPromise = assertPromise;
            }

            public override Task ConnectAsync(IChannelHandlerContext context, EndPoint remoteAddress,
                EndPoint localAddress)
            {
                _promise.TryComplete();
                return base.ConnectAsync(context, remoteAddress, localAddress);
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
                // Ensure the promise was completed before the handler method was triggered.
                if (_promise.Task.IsCompleted)
                {
                    _assertPromise.Complete();
                }
                else
                {
                    _assertPromise.SetException(new ApplicationException("connect promise should be done"));
                }
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