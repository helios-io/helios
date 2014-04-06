using System.Threading;
using Helios.Ops;

namespace Helios.Channels.Extensions
{
    /// <summary>
    /// Extension methods for working with <see cref="IExecutor"/> and <see cref="IEventLoop"/> instances
    /// </summary>
    public static class EventLoopExtensions
    {
        public static bool IsInEventLoop(this IExecutor executor)
        {
            return executor.InThread(Thread.CurrentThread);
        }
    }
}
