using Helios.Concurrency;
using Helios.Net;
using Helios.Ops;
using Helios.Ops.Executors;

namespace Helios.Channels.NIO
{
    /// <summary>
    /// <see cref="IEventLoop"/> implementation intended to be used with Non-blocking I/O (NIO) implementations of <see cref="IConnection"/>.
    /// 
    /// Uses <see cref="IFiber"/>s and a fixed size threadpool internally.
    /// </summary>
    public class NioEventLoop : ThreadedEventLoop
    {
        public NioEventLoop(int workerThreads, ReceivedDataCallback receive) : base(workerThreads)
        {
            Receive = receive;
        }

        public NioEventLoop(IExecutor internalExecutor, int workerThreads, ReceivedDataCallback receive) : base(internalExecutor, workerThreads)
        {
            Receive = receive;
        }

        public NioEventLoop(IFiber scheduler, ReceivedDataCallback receive) : base(scheduler)
        {
            Receive = receive;
        }

        public ReceivedDataCallback Receive { get; private set; }
    }
}
