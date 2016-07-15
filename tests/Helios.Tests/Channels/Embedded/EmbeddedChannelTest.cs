// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Channels;
using Helios.Channels.Embedded;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Channels.Embedded
{
    public class EmbeddedChannelTest
    {
        [Fact]
        public void TestConstructWithChannelInitializer()
        {
            var first = 1;
            var second = 2;
            IChannelHandler handler = new ChannelHandler1(first, second);
            var channel =
                new EmbeddedChannel(new ActionChannelInitializer<IChannel>(ch => { ch.Pipeline.AddLast(handler); }));
            var pipeline = channel.Pipeline;
            Assert.Same(handler, pipeline.FirstContext().Handler);
            Assert.True(channel.WriteInbound(3));
            Assert.True(channel.Finish());
            Assert.Equal(first, channel.ReadInbound<object>());
            Assert.Equal(second, channel.ReadInbound<object>());
            Assert.Null(channel.ReadInbound<object>());
        }

        [Fact]
        public async Task TestScheduledCancelledDirectly()
        {
            var ch = new EmbeddedChannel(new ChannelHandlerAdapter());

            ch.RunPendingTasks();
            var checkTask1 = ch.EventLoop.SubmitAsync(() => true);
            var checkTask2 = ch.EventLoop.SubmitAsync(() => true);
            var checkTask3 = ch.EventLoop.SubmitAsync(() => true);
            ch.RunPendingTasks();
            ch.CheckException();
            Assert.True(await checkTask1);
            Assert.True(await checkTask2);
            Assert.True(await checkTask3);
        }

        [Fact]
        public async Task TestScheduledCancelledAsync()
        {
            var ch = new EmbeddedChannel(new ChannelHandlerAdapter());
            var cts = new CancellationTokenSource();
            var cancelledTask = ch.EventLoop.SubmitAsync(() => false, cts.Token);
            var checkTask = ch.EventLoop.SubmitAsync(() => cancelledTask.IsCanceled);
            await Task.Run(() => cts.Cancel());
            ch.RunPendingTasks();
            Assert.True(await checkTask);
        }

        [Theory]
        [InlineData(3000)]
        public void TestHandlerAddedExecutedInEventLoop(int timeout)
        {
            var latch = new CountdownEvent(1);
            var ex = new AtomicReference<Exception>();
            IChannelHandler handler = new ChannelHandler3(latch, ex);
            var channel = new EmbeddedChannel(handler);
            Assert.False(channel.Finish());
            Assert.True(latch.Wait(timeout));
            var cause = ex.Value;
            if (cause != null)
            {
                throw cause;
            }
        }

        [Fact]
        public void TestConstructWithoutHandler()
        {
            var channel = new EmbeddedChannel();
            Assert.True(channel.WriteInbound(1));
            Assert.True(channel.WriteOutbound(2));
            Assert.True(channel.Finish());
            Assert.Equal(1, channel.ReadInbound<object>());
            Assert.Null(channel.ReadInbound<object>());
            Assert.Equal(2, channel.ReadOutbound<object>());
            Assert.Null(channel.ReadOutbound<object>());
        }

        [Theory]
        [InlineData(1000)]
        public void TestFireChannelInactiveAndUnregisteredOnDisconnect(int timeout)
        {
            TestFireChannelInactiveAndUnregisteredOnClose(channel => channel.DisconnectAsync(), timeout);
        }

        public void TestFireChannelInactiveAndUnregisteredOnClose(Func<IChannel, Task> action, int timeout)
        {
            var latch = new CountdownEvent(3);
            var channel = new EmbeddedChannel(new ChannelHandlerWithInactiveAndRegister(latch));
            action(channel);
            Assert.True(latch.Wait(timeout));
        }

        private class ChannelHandler1 : ChannelHandlerAdapter
        {
            private readonly int first;
            private readonly int second;

            public ChannelHandler1(int first, int second)
            {
                this.first = first;
                this.second = second;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                context.FireChannelRead(first);
                context.FireChannelRead(second);
            }
        }

        private class ChannelHandler3 : ChannelHandlerAdapter
        {
            private readonly CountdownEvent latch;
            private AtomicReference<Exception> error;

            public ChannelHandler3(CountdownEvent latch, AtomicReference<Exception> error)
            {
                this.latch = latch;
                this.error = error;
            }

            public override void HandlerAdded(IChannelHandlerContext context)
            {
                try
                {
                    Assert.True(context.Executor.InEventLoop);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    latch.Signal();
                }
            }
        }

        private class ChannelHandlerWithInactiveAndRegister : ChannelHandlerAdapter
        {
            private readonly CountdownEvent latch;

            public ChannelHandlerWithInactiveAndRegister(CountdownEvent latch)
            {
                this.latch = latch;
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                latch.Signal();
                context.Executor.Execute(() =>
                {
                    // should be executed
                    latch.Signal();
                });
            }

            public override void ChannelUnregistered(IChannelHandlerContext context)
            {
                latch.Signal();
            }
        }
    }
}