// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics.Contracts;

namespace Helios.Buffers
{
    /// <summary>
    ///     An unpooled non-blocking IO byte buffer implementation.
    /// </summary>
    public class UnpooledDirectByteBuf : AbstractReferenceCountedByteBuf
    {
        private byte[] _buffer;

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, int initialCapacity, int maxCapacity)
            : this(alloc, new byte[initialCapacity], 0, 0, maxCapacity)
        {
        }

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, byte[] initialArray, int maxCapacity)
            : this(alloc, initialArray, 0, initialArray.Length, maxCapacity)
        {
        }

        public UnpooledDirectByteBuf(IByteBufAllocator alloc, byte[] initialArray, int readerIndex, int writerIndex,
            int maxCapacity) : base(maxCapacity)
        {
            Contract.Requires(alloc != null);
            Contract.Requires(initialArray != null);
            Contract.Requires(initialArray.Length <= maxCapacity);

            Allocator = alloc;
            SetBuffer(initialArray);
            SetIndex(readerIndex, writerIndex);
        }

        public override int Capacity => _buffer.Length;

        public override ByteOrder Order => ByteOrder.LittleEndian;
        public override IByteBufAllocator Allocator { get; }

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

        public override int ArrayOffset => 0;

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
            else if (newCapacity < oldCapacity) //shrink
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

        protected override byte _GetByte(int index)
        {
            return _buffer[index];
        }

        protected override short _GetShort(int index)
        {
            return unchecked((short) (_buffer[index] | _buffer[index + 1] << 8));
        }

        protected override int _GetInt(int index)
        {
            return unchecked(_buffer[index] |
                             _buffer[index + 1] << 8 |
                             _buffer[index + 2] << 16 |
                             _buffer[index + 3] << 24);
        }

        protected override long _GetLong(int index)
        {
            unchecked
            {
                var i1 = _buffer[index] |
                         _buffer[index + 1] << 8 |
                         _buffer[index + 2] << 16 |
                         _buffer[index + 3] << 24;
                var i2 = _buffer[index + 4] |
                         _buffer[index + 5] << 8 |
                         _buffer[index + 6] << 16 |
                         _buffer[index + 7] << 24;
                return (uint) i1 | ((long) i2 << 32);
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
            _buffer.SetValue((byte) value, index);
            return this;
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            unchecked
            {
                _buffer[index] = (byte) ((ushort) value);
                _buffer[index + 1] = (byte) ((ushort) value >> 8);
            }
            return this;
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            unchecked
            {
                var unsignedValue = (uint) value;
                _buffer[index] = (byte) (value);
                _buffer[index + 1] = (byte) (unsignedValue >> 8);
                _buffer[index + 2] = (byte) (unsignedValue >> 16);
                _buffer[index + 3] = (byte) (unsignedValue >> 24);
            }
            return this;
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            unchecked
            {
                var unsignedValue = (ulong) value;
                _buffer[index] = (byte) (value);
                _buffer[index + 1] = (byte) (unsignedValue >> 8);
                _buffer[index + 2] = (byte) (unsignedValue >> 16);
                _buffer[index + 3] = (byte) (unsignedValue >> 24);
                _buffer[index + 4] = (byte) (unsignedValue >> 32);
                _buffer[index + 5] = (byte) (unsignedValue >> 40);
                _buffer[index + 6] = (byte) (unsignedValue >> 48);
                _buffer[index + 7] = (byte) (unsignedValue >> 56);
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

        public override IByteBuf Copy(int index, int length)
        {
            CheckIndex(index, length);
            var copiedArray = new byte[length];
            System.Array.Copy(_buffer, index, copiedArray, 0, length);
            return new UnpooledDirectByteBuf(Allocator, copiedArray, MaxCapacity);
        }

        public override int IoBufferCount => 1;

        public override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            this.EnsureAccessible();
            return new ArraySegment<byte>(_buffer, index, length);
        }

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length)
            => new[] {this.GetIoBuffer(index, length)};

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