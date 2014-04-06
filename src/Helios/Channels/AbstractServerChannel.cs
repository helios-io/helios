using System;
using Helios.Channels.Impl;
using Helios.Net;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels
{
    /// <summary>
    /// A bare-bones server-side <see cref="IChannel"/> implementation.
    /// 
    /// A server-side channel does not allow the following operations:
    ///     * <see cref="IChannel.Connect(INode, ChannelPromise{bool})"/>
    ///     * <see cref="IChannel.Disconnect(ChannelPromise{bool})"/>
    ///     * <see cref="IChannel.Write(NetworkData, ChannelPromise{bool})"/>
    ///     * <see cref="IChannel.Flush"/>
    ///     * And any of the shortcuts for the methods listed above.
    /// </summary>
    public abstract class AbstractServerChannel : AbstractChannel, IServerChannel
    {
        private readonly IEventLoop _childGroup;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        protected AbstractServerChannel(IEventLoop eventLoop, IEventLoop childGroup):base(null, eventLoop)
        {
            _childGroup = childGroup;
        }

        public IEventLoop ChildEventLoop()
        {
            return _childGroup;
        }

        protected override AbstractUnsafe NewUnsafe()
        {
            return new DefaultServerUnsafe(this);
        }

        #region DefaultServerUnsafe implementation

        private sealed class DefaultServerUnsafe : AbstractUnsafe
        {
            public DefaultServerUnsafe(AbstractChannel channel) : base(channel)
            {
            }

            public override void Connect(INode localAddress, INode remoteAddress, ChannelPromise<bool> connectCompletionSource)
            {
                Reject(connectCompletionSource);
            }

            protected override INode LocalAddressInternal()
            {
                return null;
            }

            protected override INode RemoteAddressInternal()
            {
                return null;
            }

            public override void Flush()
            {
                //ignore
            }

            public override void Write(NetworkData msg, ChannelPromise<bool> writeCompletionSource)
            {
                Reject(writeCompletionSource);
            }

            protected override void DoRegister()
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

            protected override void DoDeregister()
            {
                throw new System.NotImplementedException();
            }

            protected override void DoBeginRead()
            {
                throw new System.NotImplementedException();
            }

            protected override void DoWrite(ChannelOutboundBuffer buff)
            {
                throw new System.NotImplementedException();
            }

            private void Reject(ChannelPromise<bool> promise)
            {
                SafeSetFailure(promise, new NotSupportedException());
            }
        }

        #endregion
    }
}
