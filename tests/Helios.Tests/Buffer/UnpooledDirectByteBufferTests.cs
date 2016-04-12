using Helios.Buffers;

namespace Helios.Tests.Buffer
{
    
    public class UnpooledDirectByteBufferTests : ByteBufferTests
    {
        protected override IByteBuf GetBuffer(int initialCapacity)
        {
            return UnpooledByteBufAllocator.Default.Buffer(initialCapacity);
        }

        protected override IByteBuf GetBuffer(int initialCapacity, int maxCapacity)
        {
            return UnpooledByteBufAllocator.Default.Buffer(initialCapacity, maxCapacity);
        }
    }
}