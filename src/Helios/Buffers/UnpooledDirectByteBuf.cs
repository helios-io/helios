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
        private ByteBuffer _internalNioBuffer;
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
            return ByteBuffer.AllocateDirect(initialCapacity, MaxCapacity);
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
            _internalNioBuffer = null;
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
                newBuffer.WriteBytes(oldBuffer.ToArray());
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
                        SetWriterIndex(writerIndex = newCapacity);
                    }
                    oldBuffer.SetIndex(readerIndex, writerIndex);
                    newBuffer.WriteBytes(oldBuffer.ToArray());
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

        public override ByteBuffer InternalNioBuffer(int index, int length)
        {
            return (ByteBuffer)InternalNioBuffer().Clear().SetIndex(index, length);
        }

        public override IByteBuf Compact()
        {
            _buffer.Compact();
            _internalNioBuffer = null;
            SetIndex(0, _buffer.ReadableBytes);
            return this;
        }

        public override IByteBuf CompactIfNecessary()
        {
            //compact if under 10%
            if ((double) WritableBytes/Capacity <= 0.1)
            {
                return Compact();
            }
            return this;
        }

        private ByteBuffer InternalNioBuffer()
        {
            var tmpNioBuff = _internalNioBuffer;
            if (_internalNioBuffer == null)
            {
                _internalNioBuffer = tmpNioBuff = (DuplicateByteBuf)_buffer.Duplicate();
            }
            return tmpNioBuff;
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
            GetBytes(index, destination, dstIndex, length, false);
            return this;
        }

        private void GetBytes(int index, byte[] destination, int dstIndex, int length, bool isInternal)
        {
            CheckDstIndex(index, length, dstIndex, destination.Length);
            if(dstIndex < 0 || dstIndex > destination.Length - length)
                throw new IndexOutOfRangeException(string.Format(
                    "dstIndex: {0}, length: {1} (expected: range(0, {2}))", dstIndex, length, destination.Length));

            ByteBuffer tmpBuf;

            if (isInternal)
            {
                tmpBuf = InternalNioBuffer();
            }
            else
            {
                tmpBuf = (DuplicateByteBuf)_buffer.Duplicate();
            }
            
            tmpBuf.Clear().SetIndex(index, index + length);
            tmpBuf.GetBytes(index, destination, dstIndex, length);
        }

        public override IByteBuf SetByte(int index, int value)
        {
            EnsureAccessible();
            return _SetByte(index, value);
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            return _buffer.SetByte(index, value);
        }

        public override IByteBuf SetShort(int index, int value)
        {
            EnsureAccessible();
            _SetShort(index, value);
            return this;
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            _buffer.SetShort(index, value);
            return this;
        }

        public override IByteBuf SetInt(int index, int value)
        {
            EnsureAccessible();
            return _SetInt(index, value);
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            _buffer.SetInt(index, value);
            return this;
        }

        public override IByteBuf SetLong(int index, long value)
        {
            EnsureAccessible();
            return _SetLong(index, value);
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            _buffer.SetLong(index, value);
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            if (_buffer.HasArray)
            {
                src.GetBytes(srcIndex, _buffer.InternalArray(), index, length);
            }
            else
            {
                src.GetBytes(srcIndex, this, index, length);
            }
            return this;
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            var tmpBuf = InternalNioBuffer();
            tmpBuf.Clear().SetIndex(index, index + length);
            tmpBuf.SetBytes(index, src, srcIndex, length);
            return this;
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