using System.Threading.Tasks;
using Helios.Ops;
using Helios.Topology;

namespace Helios.Channels.NIO
{
    /// <summary>
    /// Abstract base class for <see cref="IChannel"/> implementations which use async / await for
    /// all I/O operations (non-blocking)
    /// </summary>
    public abstract class AbstractNioChannel : AbstractChannel
    {
        private volatile bool inputShutdown;

        private ChannelPromise<bool> connectPromise;
        private Task connectTimeoutTask;
        private INode requestedRemoteAddress;

        protected AbstractNioChannel(IChannel parent, IEventLoop loop) : base(parent, loop)
        {
        }
    }
}
