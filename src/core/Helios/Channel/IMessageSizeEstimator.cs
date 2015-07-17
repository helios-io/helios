namespace Helios.Channel
{
    /// <summary>
    /// Responsible to estimate size of a message. The size represent how much memory the message will ca. reserve in memory.
    /// </summary>
    public interface IMessageSizeEstimator
    {
        /// <summary>
        /// Creates a new handle. The handle provides the actual operations.
        /// </summary>
        /// <returns>New <see cref="IMessageSizeEstimatorHandle"/> instance</returns>
        IMessageSizeEstimatorHandle NewHandle();
    }

    public interface IMessageSizeEstimatorHandle
    {
        /// <summary>
        /// Calculate the size of the given message.
        /// </summary>
        /// <param name="message">The message for which the size should be calculated</param>
        /// <returns>The size in bytes. The returned size must be >= 0</returns>
        int Size(object message);
    }
}