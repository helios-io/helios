using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Helios.Util;

namespace Helios.Buffers
{
    /// <summary>
    /// An unpooled non-blocking IO byte buffer implementation.
    /// </summary>
    public class UnpooledDirectByteBuf : AbstractReferenceCountedByteBuf
    {
        private readonly IByteBufAllocator _alloc;

        private byte[] _buffer;

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, int initialCapacity, int maxCapacity) : this(alloc, new byte[initialCapacity], 0, 0, maxCapacity)
        {
           
        }

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, byte[] initialArray, int maxCapacity) : this(alloc, initialArray, 0, 0, maxCapacity)
        {

        }

        public UnpooledDirectByteBuf(IByteBufAllocator alloc,  byte[] initialArray, int readerIndex, int writerIndex, int maxCapacity) : base(maxCapacity)
        {
            Contract.Requires(alloc != null);
            Contract.Requires(initialArray != null);
            Contract.Requires(initialArray.Length <= maxCapacity);

            _allocator = alloc;
            SetBuffer(initialArray);
            SetIndex(readerIndex, writerIndex);
        }

        public override int Capacity => _buffer.Length;

        protected void SetBuffer(byte[] initialBuffer)
        {
            _buffer = initialBuffer;
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            if (newCapacity > MaxCapacity) throw new ArgumentOutOfRangeException("newCapacity", string.Format("capacity({0}) must be less than MaxCapacity({1})", newCapacity, MaxCapacity));
            var newBuffer = new byte[newCapacity];

            //expand
            if (newCapacity > Capacity)
            {
                Array.Copy(_buffer, ReaderIndex, newBuffer, 0, ReadableBytes);
                SetIndex(0, ReadableBytes);
            }
            else //shrink
            {
                Array.Copy(_buffer, ReaderIndex, newBuffer, 0, newCapacity);
                if (ReaderIndex < newCapacity)
                {
                    if (WriterIndex > newCapacity)
                    {
                        SetWriterIndex(newCapacity);
                    }
                    else
                    {
                        SetWriterIndex(ReadableBytes);
                    }
                    SetReaderIndex(0);
                }
                else
                {
                    SetIndex(newCapacity, newCapacity);
                }
            }

            _buffer = newBuffer;
            return this;
        }

        public override ByteOrder Order => ByteOrder.LittleEndian;

        private readonly IByteBufAllocator _allocator;
        public override IByteBufAllocator Allocator { get {return _allocator;} }

        protected override byte _GetByte(int index)
        {
            return _buffer[index];
        }

        protected override short _GetShort(int index)
        {
            return BitConverter.ToInt16(_buffer.Slice(index, 2), 0);
        }

        protected override int _GetInt(int index)
        {
            return BitConverter.ToInt32(_buffer.Slice(index, 4), 0);
        }

        protected override long _GetLong(int index)
        {
            return BitConverter.ToInt64(_buffer.Slice(index, 8), 0);
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.WritableBytes);
            destination.SetBytes(dstIndex, _buffer.Slice(index, length), 0, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.Length);
            System.Array.Copy(_buffer, index, destination, dstIndex, length);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            _buffer.SetValue((byte)value, index);
            return this;
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            unchecked
            {
                _buffer.SetRange(index, BitConverter.GetBytes((short)(value)));
            }
            return this;
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            _buffer.SetRange(index, BitConverter.GetBytes(value));
            return this;
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            _buffer.SetRange(index, BitConverter.GetBytes(value));
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.ReadableBytes);
            if (src.HasArray)
            {
                _buffer.SetRange(index, src.UnderlyingArray.Slice(srcIndex, length));
            }
            else
            {
                src.ReadBytes(_buffer, srcIndex, length);
            }
            return this;
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            System.Array.Copy(src, srcIndex, _buffer, index, length);
            return this;
        }

        public override bool HasArray
        {
            get { return true; }
        }

        public override byte[] UnderlyingArray
        {
            get { return _buffer; }
        }

        public override bool IsDirect
        {
            get { return true; }
        }

        public override IByteBuf Copy(int index, int length)
        {
            CheckIndex(index, length);
            var copiedArray = new byte[length];
            System.Array.Copy(_buffer, index, copiedArray, 0, length);
            return new UnpooledDirectByteBuf(Allocator, copiedArray, MaxCapacity);
        }

        public override int ArrayOffset => 0;

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

        protected override void Deallocate()
        {
            _buffer = null;
        }
    }
}