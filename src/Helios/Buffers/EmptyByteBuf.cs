using System;

namespace Helios.Buffers
{
    /// <summary>
    /// Represents an empty byte buffer
    /// </summary>
    public class EmptyByteBuf : AbstractByteBuf
    {
        private readonly IByteBufAllocator _alloc;

        public EmptyByteBuf(IByteBufAllocator allocator) : base(0)
        {
            _alloc = allocator;
        }

        public override int Capacity
        {
            get { return 0; }
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            throw new NotSupportedException();
        }

        public override ByteOrder Endianness => ByteOrder.LittleEndian;

        public override IByteBufAllocator Allocator
        {
            get { return _alloc; }
        }

        protected override byte _GetByte(int index)
        {
            throw new IndexOutOfRangeException();
        }

        protected override short _GetShort(int index)
        {
            throw new IndexOutOfRangeException();
        }

        protected override int _GetInt(int index)
        {
            throw new IndexOutOfRangeException();
        }

        protected override long _GetLong(int index)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        public override bool HasArray
        {
            get { return false; }
        }

        public override byte[] InternalArray()
        {
            throw new NotSupportedException();
        }

        public override bool IsDirect
        {
            get { return true; }
        }

        public override IByteBuf Copy(int index, int length)
        {
            return this;
        }

        public override IByteBuf Unwrap()
        {
            return null;
        }

        public override IByteBuf Compact()
        {
            return this;
        }

        public override IByteBuf CompactIfNecessary()
        {
            return this;
        }
    }
}
