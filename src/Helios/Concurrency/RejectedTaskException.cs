using System;

namespace Helios.Concurrency
{
    /// <summary>
    /// An exception thrown when a <see cref="IPausableEventExecutor"/> is asked to a queue a task
    /// when it is no longer accepting work.
    /// </summary>
    public class RejectedTaskException : Exception
    {
        public RejectedTaskException() : base("Not accepting new work at this time!")
        {
            
        }

        public static readonly RejectedTaskException Instance = new RejectedTaskException();
    }
}