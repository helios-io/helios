using Helios.Buffers;

namespace Helios.Channels
{
    /// <summary>
    /// A <see cref="IRecvByteBufAllocator"/> that always yields the same buffer size prediction;
    /// ignores feedback from the I/O thread.
    /// </summary>
    public sealed class FixedRecvByteBufAllocator : IRecvByteBufAllocator
    {
        public static readonly FixedRecvByteBufAllocator Default = new FixedRecvByteBufAllocator(4 * 1024);

        sealed class Handle : IRecvByteBufferAllocatorHandle
        {
            private readonly int _bufferSize;

            public Handle(int bufferSize)
            {
                _bufferSize = bufferSize;
            }

            public IByteBuf Allocate(IByteBufAllocator allocator)
            {
                return allocator.Buffer(_bufferSize);
            }

            public int Guess()
            {
                return _bufferSize;
            }

            public void Record(int actualReadBytes)
            {
                // no-op
            }
        }

        private readonly IRecvByteBufferAllocatorHandle _handle;

        public FixedRecvByteBufAllocator(int bufferSize)
        {
            _handle = new Handle(bufferSize);
        }

        public IRecvByteBufferAllocatorHandle NewHandle()
        {
            return _handle;
        }
    }
}