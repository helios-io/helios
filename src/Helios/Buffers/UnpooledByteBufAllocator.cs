namespace Helios.Buffers
{
    /// <summary>
    /// Unpooled implementation of <see cref="IByteBufAllocator"/>.
    /// </summary>
    public class UnpooledByteBufAllocator : AbstractByteBufAllocator
    {
        /// <summary>
        /// Default instance
        /// </summary>
        public static readonly UnpooledByteBufAllocator Default = new UnpooledByteBufAllocator();

        protected override IByteBuf NewDirectBuffer(int initialCapacity, int maxCapacity)
        {
            return new UnpooledDirectByteBuf(this, initialCapacity, maxCapacity);
        }
    }
}
