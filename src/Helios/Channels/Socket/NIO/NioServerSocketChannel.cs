using System;
using Helios.Channels.NIO;
using Helios.Net;
using Helios.Ops;
using Helios.Reactor;
using Helios.Topology;

namespace Helios.Channels.Socket.NIO
{
    /// <summary>
    /// Non-blocking I/O implementation of a <see cref="IServerSocketChannel"/> over TCP/IP
    /// </summary>
    public class NioServerSocketChannel : AbstractNioByteServerChannel, IServerSocketChannel
    {
 
        public NioServerSocketChannel(IChannel parent, IEventLoop loop, IEventLoop childGroup, IConnection connection)
            : base(parent, loop, childGroup, connection)
        {
            
        }

        protected override bool DoConnect(INode remoteAddress, INode localAddress)
        {
            throw new NotSupportedException("Connect is not supported on server channels");
        }

        protected override void DoFinishConnect()
        {
            throw new NotSupportedException("Connect is not supported on server channels");
        }

        protected override int DoWriteBytes(byte[] buff)
        {
            throw new NotSupportedException("WriteBytes is not supported on server channels");
        }

        public new IServerSocketChannelConfig Config { get; internal set; }
    }
}
