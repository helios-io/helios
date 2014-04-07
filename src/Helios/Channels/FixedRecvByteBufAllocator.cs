namespace Helios.Channels
{
    /// <summary>
    /// Fixed size implemenation of a ByteBuf allocator
    /// </summary>
    public sealed class FixedRecvByteBufAllocator : IRecvByteBufAllocator
    {
        private readonly HandleImpl _handle;

        public FixedRecvByteBufAllocator(int bufferSize)
        {
            _handle = new HandleImpl(bufferSize);
        }

        public IByteBufHandle NewHandle()
        {
            return _handle;
        }

        #region HandleImpl

        private sealed class HandleImpl : IByteBufHandle
        {
            private readonly int _bufferSize;

            public HandleImpl(int bufferSize)
            {
                _bufferSize = bufferSize;
            }

            public byte[] Allocate()
            {
                return new byte[_bufferSize];
            }

            public int Guess()
            {
                return _bufferSize;
            }

            public void Record(int actualReadBytes)
            {
               //no-op
            }
        }

        #endregion
    }
}