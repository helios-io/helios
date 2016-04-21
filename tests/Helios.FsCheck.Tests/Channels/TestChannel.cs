using System;
using System.Net;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Concurrency;

namespace Helios.FsCheck.Tests.Channels
{
    /// <summary>
    /// Internal <see cref="IChannel"/> implementation used for lightweight testing.
    /// 
    /// Mostly for testing things that go INSIDE the channel itself.
    /// </summary>
    internal class TestChannel : IChannel
    {

        public static readonly TestChannel Instance = new TestChannel();

        private TestChannel()
        {
            Configuration = new DefaultChannelConfiguration(this);
            Configuration.WriteBufferHighWaterMark = ChannelOutboundBufferSpecs.WriteHighWaterMark;
            Configuration.WriteBufferLowWaterMark = ChannelOutboundBufferSpecs.WriteLowWaterMark;
        }

        public IByteBufAllocator Allocator { get; }
        public IEventLoop EventLoop { get; }
        public IChannel Parent { get; }
        public bool DisconnectSupported { get; }
        public bool Open { get; }
        public bool Active { get; }
        public bool Registered { get; }
        public EndPoint LocalAddress { get; }
        public EndPoint RemoteAddress { get; }
        public bool IsWritable { get; }
        public IChannelUnsafe Unsafe { get; }
        public IChannelPipeline Pipeline { get; }
        public IChannelConfiguration Configuration { get; }
        public Task CloseCompletion { get; }
        public Task DeregisterAsync()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public IChannel Read()
        {
            return this;
        }

        public Task WriteAsync(object message)
        {
            throw new NotImplementedException();
        }

        public IChannel Flush()
        {
            throw new NotImplementedException();
        }

        public Task WriteAndFlushAsync(object message)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
        {
            throw new NotImplementedException();
        }

        public Task ConnectAsync(EndPoint remoteAddress)
        {
            throw new NotImplementedException();
        }

        public Task BindAsync(EndPoint localAddress)
        {
            throw new NotImplementedException();
        }
    }
}