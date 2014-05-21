using System;

namespace Helios.Buffers
{
    /// <summary>
    /// An unpooled non-blocking IO byte buffer implementation.
    /// </summary>
    public class UnpooledDirectByteBuf : ByteBufBase
    {
        private readonly IByteBufAllocator _alloc;

        private ByteBuffer _buffer;
        private int _capacity;

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, int initialCapacity, int maxCapacity) : base(maxCapacity)
        {
            if(alloc == null) throw new ArgumentNullException("alloc");
            if(initialCapacity < 0) throw new ArgumentOutOfRangeException("initialCapacity", "initialCapacity must be at least 0");
            if(maxCapacity < 0) throw new ArgumentOutOfRangeException("maxCapacity", "maxCapacity must be at least 0");
            if(initialCapacity > maxCapacity) throw new ArgumentException(string.Format("initialCapacity {0} must be less than maxCapacity {1}", initialCapacity, maxCapacity));

            _alloc = alloc;
            _capacity = initialCapacity;
            //SetByteBuffer();
        }

        private void SetByteBuffer(ByteBuffer buffer)
        {
            
        }

        public override int Capacity
        {
            get { return _capacity; }
        }

        public override IByteBuf AdjustCapacity(int capacity)
        {
            throw new NotImplementedException();
        }

        public override IByteBufAllocator Allocator
        {
            get { throw new NotImplementedException(); }
        }

        protected override byte _GetByte(int index)
        {
            throw new NotImplementedException();
        }

        protected override short _GetShort(int index)
        {
            throw new NotImplementedException();
        }

        protected override int _GetInt(int index)
        {
            throw new NotImplementedException();
        }

        protected override long _GetLong(int index)
        {
            throw new NotImplementedException();
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            throw new NotImplementedException();
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            throw new NotImplementedException();
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            throw new NotImplementedException();
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            throw new NotImplementedException();
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            throw new NotImplementedException();
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            throw new NotImplementedException();
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            throw new NotImplementedException();
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            throw new NotImplementedException();
        }

        public override bool HasArray
        {
            get { return false; }
        }

        public override byte[] InternalArray()
        {
            throw new NotSupportedException("direct buffer");
        }
    }
}