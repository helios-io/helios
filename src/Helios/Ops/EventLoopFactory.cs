using Helios.Net;
using Helios.Ops.Executors;

namespace Helios.Ops
{
    /// <summary>
    /// Static factory class for creating <see cref="IEventLoop"/> instances
    /// </summary>
    public static class EventLoopFactory
    {
        public static IEventLoop CreateThreadedEventLoop(int defaultSize = 2, IExecutor internalExecutor = null)
        {
            return new ThreadedEventLoop(internalExecutor, defaultSize);
        }

        public static NetworkEventLoop CreateNetworkEventLoop(int defaultSize = 2, IExecutor internalExecutor = null)
        {
            return new NetworkEventLoop(internalExecutor, defaultSize);
        }
    }
}
