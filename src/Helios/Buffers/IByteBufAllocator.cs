namespace Helios.Buffers
{
    /// <summary>
    /// Thread-safe interface for allocating <see cref="IByteBuf"/> instances for use inside Helios reactive I/O
    /// </summary>
    public interface IByteBufAllocator
    {
        IByteBuf Buffer();

        IByteBuf Buffer(int initialCapcity);

        IByteBuf Buffer(int initialCapacity, int maxCapacity);
    }
}