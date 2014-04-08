using Helios.Net;
using Helios.Ops;

namespace Helios.Channels.NIO
{
    /// <summary>
    /// <see cref="IServerChannel"/> implementation of a <see cref="AbstractNioByteChannel"/>
    /// </summary>
    public abstract class AbstractNioByteServerChannel : AbstractNioByteChannel, IServerChannel
    {
        private readonly IEventLoop _childGroup;

        protected AbstractNioByteServerChannel(IChannel parent, IEventLoop loop, IEventLoop childGroup, IConnection connection) : base(parent, loop, connection)
        {
            _childGroup = childGroup;
        }

        public IEventLoop ChildEventLoop()
        {
            return _childGroup;
        }
    }
}
