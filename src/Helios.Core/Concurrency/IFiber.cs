using System;

namespace Helios.Core.Concurrency
{
    /// <summary>
    /// Interface for lightweight threading and execution
    /// </summary>
    public interface IFiber
    {
        void Add(Action op);

        /// <summary>
        /// Shuts down this Fiber within the allotted timeframe
        /// </summary>
        /// <param name="gracePeriod">The amount of time given for currently executing tasks to complete</param>
        void Shutdown(TimeSpan gracePeriod);

        /// <summary>
        /// Performs a hard-stop on the Fiber - no more actions can be executed
        /// </summary>
        void Stop();
    }
}
