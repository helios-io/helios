using System;

namespace Helios.Ops
{
    /// <summary>
    /// Interface used for creating chained eventloops
    /// for processing network streams / events
    /// </summary>
    public interface IEventLoop : IExecutor, IDisposable
    {
        /// <summary>
        /// Was this event loop disposed?
        /// </summary>
        bool WasDisposed { get; }
    }
}
