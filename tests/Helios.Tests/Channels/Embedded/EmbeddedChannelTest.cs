using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        class ChannelHandler1 : ChannelHandlerAdapter
        {
            readonly int first;
            readonly int second;

            public ChannelHandler1(int first, int second)
            {
                this.first = first;
                this.second = second;
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                context.FireChannelRead(this.first);
                context.FireChannelRead(this.second);
            }
        }

        [Fact]
        public void TestConstructWithChannelInitializer()
        {
            int first = 1;
            int second = 2;
            IChannelHandler handler = new ChannelHandler1(first, second);
            EmbeddedChannel channel =
                new EmbeddedChannel(new ActionChannelInitializer<IChannel>(ch => { ch.Pipeline.AddLast(handler); }));
            IChannelPipeline pipeline = channel.Pipeline;
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
            Task<bool> checkTask1 = ch.EventLoop.SubmitAsync(() => true);
            Task<bool> checkTask2 = ch.EventLoop.SubmitAsync(() => true);
            Task<bool> checkTask3 = ch.EventLoop.SubmitAsync(() => true);
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

        class ChannelHandler3 : ChannelHandlerAdapter
        {
            readonly CountdownEvent latch;
            AtomicReference<Exception> error;

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
                    this.error = ex;
                }
                finally
                {
                    this.latch.Signal();
                }
            }
        }

        [Theory]
        [InlineData(3000)]
        public void TestHandlerAddedExecutedInEventLoop(int timeout)
        {
            CountdownEvent latch = new CountdownEvent(1);
            AtomicReference<Exception> ex = new AtomicReference<Exception>();
            IChannelHandler handler = new ChannelHandler3(latch, ex);
            EmbeddedChannel channel = new EmbeddedChannel(handler);
            Assert.False(channel.Finish());
            Assert.True(latch.Wait(timeout));
            Exception cause = ex.Value;
            if (cause != null)
            {
                throw cause;
            }
        }

        [Fact]
        public void TestConstructWithoutHandler()
        {
            EmbeddedChannel channel = new EmbeddedChannel();
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
            this.TestFireChannelInactiveAndUnregisteredOnClose(channel => channel.DisconnectAsync(), timeout);
        }

        public void TestFireChannelInactiveAndUnregisteredOnClose(Func<IChannel, Task> action, int timeout)
        {
            CountdownEvent latch = new CountdownEvent(3);
            EmbeddedChannel channel = new EmbeddedChannel(new ChannelHandlerWithInactiveAndRegister(latch));
            action(channel);
            Assert.True(latch.Wait(timeout));
        }

        class ChannelHandlerWithInactiveAndRegister : ChannelHandlerAdapter
        {
            CountdownEvent latch;

            public ChannelHandlerWithInactiveAndRegister(CountdownEvent latch)
            {
                this.latch = latch;
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                this.latch.Signal();
                context.Executor.Execute(() =>
                {
                    // should be executed
                    this.latch.Signal();
                });
            }

            public override void ChannelUnregistered(IChannelHandlerContext context)
            {
                this.latch.Signal();
            }
        }
    }
}
