using Helios.Channels.NIO;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.Socket.NIO
{
    public class NioSocketChannel : AbstractNioByteChannel, ISocketChannel
    {
        public NioSocketChannel(IChannel parent, IEventLoop loop, IConnection connection) : base(parent, loop, connection)
        {
        }

        protected override bool DoConnect(INode remoteAddress, INode localAddress)
        {
            throw new System.NotImplementedException();
        }

        protected override void DoFinishConnect()
        {
            throw new System.NotImplementedException();
        }

        protected override int DoWriteBytes(byte[] buff)
        {
            throw new System.NotImplementedException();
        }

        public new IServerSocketChannel Parent { get { return (IServerSocketChannel) base.Parent; } }
        public new ISocketChannelConfig Config { get; internal set; }
        public bool InputShutdown { get; private set; }
        public bool OutputShutdown { get; private set; }
        public ChannelFuture ShutDownOutput()
        {
            throw new System.NotImplementedException();
        }

        public ChannelFuture ShutDownOutput(ChannelPromise<bool> future)
        {
            throw new System.NotImplementedException();
        }
    }
}
