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
        private byte[] _buffer;

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, int initialCapacity, int maxCapacity) : this(alloc, new byte[initialCapacity], 0, 0, maxCapacity)
        {
           
        }

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, byte[] initialArray, int maxCapacity) : this(alloc, initialArray, 0, initialArray.Length, maxCapacity)
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
            EnsureAccessible();
            Contract.Requires(newCapacity >= 0 && newCapacity <= MaxCapacity);
            
            var oldCapacity = _buffer.Length;
            //expand
            if (newCapacity > oldCapacity)
            {
                var newBuffer = new byte[newCapacity];
                System.Array.Copy(_buffer, 0, newBuffer, 0, _buffer.Length);
                SetBuffer(newBuffer);
            }
            else if(newCapacity < oldCapacity) //shrink
            {
                var newBuffer = new byte[newCapacity];
                var readerIndex = ReaderIndex;
               
                if (readerIndex < newCapacity)
                {
                    var writerIndex = WriterIndex;
                    if (writerIndex > newCapacity)
                    {
                        SetWriterIndex(writerIndex = newCapacity);
                    }
                    System.Array.Copy(_buffer, readerIndex, newBuffer, 0, writerIndex - readerIndex);
                }
                else
                {
                    SetIndex(newCapacity, newCapacity);
                }
                SetBuffer(newBuffer);
            }

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
            return unchecked((short)(_buffer[index] << 8 | _buffer[index + 1]));
        }

        protected override int _GetInt(int index)
        {
            return unchecked(_buffer[index] << 24 |
                _buffer[index + 1] << 16 |
                _buffer[index + 2] << 8 |
                _buffer[index + 3]);
        }

        protected override long _GetLong(int index)
        {
            unchecked
            {
                int i1 = _buffer[index] << 24 |
                    _buffer[index + 1] << 16 |
                    _buffer[index + 2] << 8 |
                    _buffer[index + 3];
                int i2 = _buffer[index + 4] << 24 |
                    _buffer[index + 5] << 16 |
                    _buffer[index + 6] << 8 |
                    _buffer[index + 7];
                return (uint)i2 | ((long)i1 << 32);
            }
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.WritableBytes);
            if (destination.HasArray)
            {
                GetBytes(index, destination.Array, destination.ArrayOffset + dstIndex, length);
            }
            else
            {
                destination.SetBytes(dstIndex, Array, index, length);
            }
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
                _buffer[index] = (byte)((ushort)value >> 8);
                _buffer[index + 1] = (byte)value;
            }
            return this;
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            unchecked
            {
                uint unsignedValue = (uint)value;
                _buffer[index] = (byte)(unsignedValue >> 24);
                _buffer[index + 1] = (byte)(unsignedValue >> 16);
                _buffer[index + 2] = (byte)(unsignedValue >> 8);
                _buffer[index + 3] = (byte)value;
            }
            return this;
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)value;
                _buffer[index] = (byte)(unsignedValue >> 56);
                _buffer[index + 1] = (byte)(unsignedValue >> 48);
                _buffer[index + 2] = (byte)(unsignedValue >> 40);
                _buffer[index + 3] = (byte)(unsignedValue >> 32);
                _buffer[index + 4] = (byte)(unsignedValue >> 24);
                _buffer[index + 5] = (byte)(unsignedValue >> 16);
                _buffer[index + 6] = (byte)(unsignedValue >> 8);
                _buffer[index + 7] = (byte)value;
            }
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Capacity);
            if (src.HasArray)
            {
                SetBytes(index, src.Array, src.ArrayOffset + srcIndex, length);
            }
            else
            {
                src.GetBytes(srcIndex, Array, index, length);
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

        public override byte[] Array
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