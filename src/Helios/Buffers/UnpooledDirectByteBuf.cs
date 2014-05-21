using System;

namespace Helios.Buffers
{
    /// <summary>
    /// An unpooled non-blocking IO byte buffer implementation.
    /// </summary>
    public class UnpooledDirectByteBuf : AbstractByteBuf
    {
        private readonly IByteBufAllocator _alloc;

        private ByteBuffer _buffer;
        private int _capacity;
        private bool _doNotFree;

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, int initialCapacity, int maxCapacity) : base(maxCapacity)
        {
            if(alloc == null) throw new ArgumentNullException("alloc");
            if(initialCapacity < 0) throw new ArgumentOutOfRangeException("initialCapacity", "initialCapacity must be at least 0");
            if(maxCapacity < 0) throw new ArgumentOutOfRangeException("maxCapacity", "maxCapacity must be at least 0");
            if(initialCapacity > maxCapacity) throw new ArgumentException(string.Format("initialCapacity {0} must be less than maxCapacity {1}", initialCapacity, maxCapacity));

            _alloc = alloc;
            _capacity = initialCapacity;
            SetByteBuffer(ByteBuffer.AllocateDirect(initialCapacity));
        }

        protected ByteBuffer AllocateDirect(int initialCapacity)
        {
            return ByteBuffer.AllocateDirect(initialCapacity);
        }

        private void SetByteBuffer(ByteBuffer buffer)
        {
            var oldBuffer = _buffer;
            if (oldBuffer != null)
            {
                if (_doNotFree)
                {
                    _doNotFree = false;
                }
                else
                {
                    oldBuffer = null; //mark for GC
                }
            }
            _buffer = buffer;
            _capacity = buffer.Capacity;
        }

        public override int Capacity
        {
            get { return _capacity; }
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            EnsureAccessible();
            if(newCapacity < 0 || newCapacity > MaxCapacity)
                throw new ArgumentOutOfRangeException("newCapacity", string.Format("newCapacity: {0}", newCapacity));

            var readerIndex = ReaderIndex;
            var writerIndex = WriterIndex;

            var oldCapacity = Capacity;
            if (newCapacity > oldCapacity)
            {
                var oldBuffer = _buffer;
                var newBuffer = AllocateDirect(newCapacity);
                oldBuffer.SetIndex(0, oldBuffer.Capacity);
                newBuffer.SetIndex(0, oldBuffer.Capacity);
                newBuffer.WriteBytes(oldBuffer);
                newBuffer.Clear();
                SetByteBuffer(newBuffer);
            }
            else if (newCapacity < oldCapacity)
            {
                var oldBuffer = _buffer;
                var newBuffer = AllocateDirect(newCapacity);
                if (ReaderIndex < newCapacity)
                {
                    if (WriterIndex > newCapacity)
                    {
                        SetWriterIndex(newCapacity);
                    }
                    oldBuffer.SetIndex(ReaderIndex, WriterIndex);
                    newBuffer.SetIndex(ReaderIndex, WriterIndex);
                    newBuffer.WriteBytes(oldBuffer);
                    newBuffer.Clear();
                }
                else
                {
                    SetIndex(newCapacity, newCapacity);
                }
                SetByteBuffer(newBuffer);
            }

            return this;
        }

        public override IByteBufAllocator Allocator
        {
            get { return _alloc; }
        }

        protected override byte _GetByte(int index)
        {
            EnsureAccessible();
            return _buffer.GetByte(index);
        }

        protected override short _GetShort(int index)
        {
            EnsureAccessible();
            return _buffer.GetShort(index);
        }

        protected override int _GetInt(int index)
        {
            EnsureAccessible();
            return _buffer.GetInt(index);
        }

        protected override long _GetLong(int index)
        {
            EnsureAccessible();
            return _buffer.GetLong(index);
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.Capacity);
            if (destination.HasArray)
            {
                GetBytes(index, destination.InternalArray(), dstIndex, length);
            }
            else
            {
                destination.SetBytes(dstIndex, this, index, length);
            }
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            throw new NotImplementedException();
        }

        /* currently are not using the "isInternal" field, since we don't do much with shared buffer pools */
        private void GetBytes(int index, byte[] destination, int dstIndex, int length, bool isInternal)
        {
            CheckDstIndex(index, length, dstIndex, destination.Length);
            if(dstIndex < 0 || dstIndex > destination.Length - length)
                throw new IndexOutOfRangeException(string.Format(
                    "dstIndex: {0}, length: {1} (expected: range(0, {2}))", dstIndex, length, destination.Length));

            var tmpBuf = (ByteBuffer)_buffer.Duplicate();
            tmpBuf.Clear().SetIndex(index, index + length);
            tmpBuf.GetBytes(index, destination, dstIndex, length);
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

        public override bool IsDirect
        {
            get { return true; }
        }

        public override IByteBuf Unwrap()
        {
            return null;
        }
    }
}