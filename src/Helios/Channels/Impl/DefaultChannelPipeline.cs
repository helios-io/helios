using System;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Topology;

namespace Helios.Channels.Impl
{
    public class DefaultChannelPipeline : ChannelPipeline
    {
        public override ChannelPipeline FireChannelRegistered()
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline FireChannelActive()
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline FireChannelInactive()
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline FireExceptionCaught(Exception ex)
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline FireChannelRead(NetworkData message)
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline FireChannelWritabilityChanged()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Bind(INode localAddress)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Bind(INode localAddress, TaskCompletionSource<bool> bindCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Connect(INode remoteAddress)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Connect(INode remoteAddress, INode localAddress)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Connect(INode remoteAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Connect(INode remoteAddress, INode localAddress, TaskCompletionSource<bool> connectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Disconnect()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Disconnect(TaskCompletionSource<bool> disconnectCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Close()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Close(TaskCompletionSource<bool> closeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline Read()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Write(NetworkData message)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> Write(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override ChannelPipeline Flush()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> WriteAndFlush(NetworkData message, TaskCompletionSource<bool> writeCompletionSource)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> WriteAndFlush(NetworkData message)
        {
            throw new NotImplementedException();
        }
    }
}