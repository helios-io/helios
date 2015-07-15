using Helios.Channel;
using System;

namespace Helios.Buffers
{
    public class FixedRecvByteBufAllocator : IRecvByteBufAllocator
    {
        private class HandleImpl : IRecvByteBufAllocatorHandle
        {
            private readonly int _bufferSize;

            public HandleImpl(int bufferSize)
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
                
            }
        }

        private readonly IRecvByteBufAllocatorHandle _handle;

        public FixedRecvByteBufAllocator(int bufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", "Cannot be negative value");

            _handle = new HandleImpl(bufferSize);
        }

        public IRecvByteBufAllocatorHandle NewHandle()
        {
            return _handle;
        }
    }
}