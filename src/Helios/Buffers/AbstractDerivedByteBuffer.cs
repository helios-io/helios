namespace Helios.Buffers
{
    /// <summary>
    /// Abstract base class for buffer implementation that wrap other <see cref="IByteBuf"/>s internally,
    /// such as <see cref="DuplicateByteBuf"/> and more.
    /// </summary>
    public abstract class AbstractDerivedByteBuffer : AbstractByteBuf
    {
        protected AbstractDerivedByteBuffer(int maxCapacity) : base(maxCapacity)
        {
        }
    }
}