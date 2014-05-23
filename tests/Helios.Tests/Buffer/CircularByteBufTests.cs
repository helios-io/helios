using Helios.Buffers;
using NUnit.Framework;

namespace Helios.Tests.Buffer
{
    [TestFixture]
    public class CircularByteBufTests : ByteBufferTests
    {
        protected override IByteBuf GetBuffer(int initialCapacity)
        {
            return CircularByteBuf.AllocateDirect(initialCapacity);
        }

        protected override IByteBuf GetBuffer(int initialCapacity, int maxCapacity)
        {
            return CircularByteBuf.AllocateDirect(initialCapacity, maxCapacity);
        }
    }
}