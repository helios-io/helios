using System;

namespace Helios.Channels
{
    /// <summary>
    /// Allocates a new receive buffer whose capacity is probably large enough
    /// to read all inbound data and small enough not to waste space
    /// </summary>
    public interface IRecvByteBufAllocator
    {
        /// <summary>
        /// Creates a new handle. The handle provides the actual operations and keeps
        /// internal information which is required for predicting optimal buffer capacity.
        /// </summary>
        IByteBufHandle NewHandle();
    }

    public interface IByteBufHandle
    {
        /// <summary>
        /// Creates a new receive buffer
        /// </summary>
        byte[] Allocate();

        /// <summary>
        /// Doesn't allocate any byte arrays, but makes a guess as to how large
        /// the next one will be
        /// </summary>
        int Guess();

        /// <summary>
        /// Records the actual number of read bytes used
        /// </summary>
        void Record(int actualReadBytes);
    }
}
