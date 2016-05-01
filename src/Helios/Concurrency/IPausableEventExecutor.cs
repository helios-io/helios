namespace Helios.Concurrency
{
    /// <summary>
    /// An <see cref="IEventExecutor"/> that can reject new work while paused.
    /// </summary>
    public interface IPausableEventExecutor : IWrappedEventExecutor
    {
        /// <summary>
        /// Pause - may throw a <see cref="RejectedTaskException"/> if work is queued while paused.
        /// </summary>
        void RejectNewTasks();

        /// <summary>
        /// Unpause.
        /// </summary>
        void AcceptNewTasks();

        /// <summary>
        /// Returns true if paused, false otherwise.
        /// </summary>
        bool IsAcceptingNewTasks { get; }
    }
}