using Helios.Buffers;

namespace Helios.Channel
{
    /// <summary>
    /// Allocates a new receive buffer whose capacity is probably large enough to read all inbound data and small enough
    /// not to waste its space.
    /// </summary>
    public interface IRecvByteBufAllocator
    {
        /// <summary>
        /// Creates a new handle.  The handle provides the actual operations and keeps the internal information which is
        /// required for predicting an optimal buffer capacity.
        /// </summary>
        /// <returns>New <see cref="IRecvByteBufAllocatorHandle"/> instance</returns>
        IRecvByteBufAllocatorHandle NewHandle();
    }

    public interface IRecvByteBufAllocatorHandle
    {
        /// <summary>
        /// Creates a new receive buffer whose capacity is probably large enough to read all inbound data and small
        /// enough not to waste its space.
        /// </summary>
        /// <param name="allocator"><see cref="IByteBufAllocator"/> implementation</param>
        /// <returns>New <see cref="IByteBuf"/> instance</returns>
        IByteBuf Allocate(IByteBufAllocator allocator);

        /// <summary>
        /// Similar to <see cref="Allocate"/> except that it does not allocate anything but just tells the
        /// capacity.
        /// </summary>
        /// <returns></returns>
        int Guess();

        /// <summary>
        /// Records the the actual number of read bytes in the previous read operation so that the allocator allocates
        /// the buffer with potentially more correct capacity.
        /// </summary>
        /// <param name="actualReadBytes">The actual number of read bytes in the previous read operation</param>
        void Record(int actualReadBytes);
    }
}