using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.NIO
{

    /// <summary>
    /// A <see cref="AbstractNioChannel"/> which uses <see cref="byte"/>s as the underlying message-passing store
    /// </summary>
    public abstract class AbstractNioByteChannel : AbstractNioChannel
    {
        protected AbstractNioByteChannel(IChannel parent, IEventLoop loop, IConnection connection) : base(parent, loop, connection)
        {
        }

        #region NioByteUnsafe implementation

        private sealed class NioByteUnsafe : AbstractNioUnsafe
        {
            public NioByteUnsafe(AbstractNioChannel channel) : base(channel)
            {
            }

            protected override INode LocalAddressInternal()
            {
                throw new System.NotImplementedException();
            }

            protected override INode RemoteAddressInternal()
            {
                throw new System.NotImplementedException();
            }

            protected override void DoBind(INode localAddress)
            {
                throw new System.NotImplementedException();
            }

            protected override void DoDisconnect()
            {
                throw new System.NotImplementedException();
            }

            protected override void DoClose()
            {
                throw new System.NotImplementedException();
            }

            protected override void DoWrite(ChannelOutboundBuffer buff)
            {
                throw new System.NotImplementedException();
            }

            public override void Read()
            {
                var config = Channel.Config;
                var pipeline = Channel.Pipeline;
                var maxMessagesRead = config.MaxMessagesPerRead;
            }
        }

        #endregion
    }
}
