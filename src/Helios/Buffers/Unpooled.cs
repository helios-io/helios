using Helios.Concurrency;

namespace Helios.Buffers
{
    /// <summary>
    /// Utility class for managing and creating unpooled buffers
    /// </summary>
    public static class Unpooled
    {
        private static readonly IByteBufAllocator Alloc = UnpooledByteBufAllocator.Default;

        public static readonly IByteBuf Empty = Alloc.Buffer(0, 0);

        public static IByteBuf Buffer()
        {
            return Alloc.Buffer();
        }

        public static IByteBuf Buffer(int initialCapacity)
        {
            return Alloc.Buffer(initialCapacity);
        }

        public static IByteBuf Buffer(int initialCapacity, int maxCapacity)
        {
            return Alloc.Buffer(initialCapacity, maxCapacity);
        }

        public static IByteBuf WrappedBuffer(byte[] bytes)
        {
           return WrappedBuffer(bytes, 0, bytes.Length);
        }

        public static IByteBuf WrappedBuffer(byte[] bytes, int index, int length)
        {
            if (bytes.Length == 0)
                return Empty;
            return Alloc.Buffer(bytes.Length).WriteBytes(bytes, index, length);
        }
    }
}
