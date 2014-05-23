namespace Helios.Buffers
{
    /// <summary>
    /// ByteBuf allocators that reserves a special <see cref="IByteBuf"/> for operations that have to persist
    /// through requests, but otherwise allows continuous use of normal bytebuf allocation
    /// </summary>
    public class ChannelReservedByteBufAllocator : AbstractByteBufAllocator
    {
        protected override IByteBuf NewDirectBuffer(int initialCapacity, int maxCapacity)
        {
            throw new System.NotImplementedException();
        }
    }
}
