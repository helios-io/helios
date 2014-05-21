namespace Helios.Buffers
{
    /// <summary>
    /// Thread-safe interface for allocating <see cref="IByteBuffer"/> instances for use inside Helios reactive I/O
    /// </summary>
    public interface IByteBufferAllocator
    {
        IByteBuffer Buffer();

        IByteBuffer Buffer(int initialCapcity);

        IByteBuffer Buffer(int initialCapacity, int maxCapacity);
    }
}