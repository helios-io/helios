// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Buffers
{
    public sealed class SlicedByteBuffer : AbstractDerivedByteBuffer
    {
        private readonly int _adjustment;
        private readonly IByteBuf _buffer;
        private readonly int _length;

        public SlicedByteBuffer(IByteBuf buffer, int index, int length)
            : base(length)
        {
            if (index < 0 || index > buffer.Capacity - length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), buffer + ".slice(" + index + ", " + length + ')');
            }

            var slicedByteBuf = buffer as SlicedByteBuffer;
            if (slicedByteBuf != null)
            {
                this._buffer = slicedByteBuf._buffer;
                _adjustment = slicedByteBuf._adjustment + index;
            }
            else if (buffer is DuplicateByteBuf)
            {
                this._buffer = buffer.Unwrap();
                _adjustment = index;
            }
            else
            {
                this._buffer = buffer;
                _adjustment = index;
            }
            this._length = length;

            SetWriterIndex(length);
        }

        public override IByteBufAllocator Allocator
        {
            get { return _buffer.Allocator; }
        }

        public override ByteOrder Order
        {
            get { return _buffer.Order; }
        }

        public override int Capacity
        {
            get { return _length; }
        }

        public override int IoBufferCount => this.Unwrap().IoBufferCount;

        public override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            this.CheckIndex(index, length);
            return this.Unwrap().GetIoBuffer(index + this._adjustment, length);
        }

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            this.CheckIndex(index, length);
            return this.Unwrap().GetIoBuffers(index + this._adjustment, length);
        }

        public override bool HasArray
        {
            get { return _buffer.HasArray; }
        }

        public override byte[] Array
        {
            get { return _buffer.Array; }
        }

        public override bool IsDirect => _buffer.IsDirect;

        public override int ArrayOffset
        {
            get { return _buffer.ArrayOffset + _adjustment; }
        }

        public override IByteBuf Unwrap()
        {
            return _buffer;
        }

        public override IByteBuf Compact()
        {
            throw new NotImplementedException();
        }

        public override IByteBuf CompactIfNecessary()
        {
            throw new NotImplementedException();
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            throw new NotSupportedException("sliced buffer");
        }

        protected override byte _GetByte(int index)
        {
            return _buffer.GetByte(index + _adjustment);
        }

        protected override short _GetShort(int index)
        {
            return _buffer.GetShort(index + _adjustment);
        }

        protected override int _GetInt(int index)
        {
            return _buffer.GetInt(index + _adjustment);
        }

        protected override long _GetLong(int index)
        {
            return _buffer.GetLong(index + _adjustment);
        }

        public override IByteBuf Duplicate()
        {
            var duplicate = _buffer.Slice(_adjustment, _length);
            duplicate.SetIndex(ReaderIndex, WriterIndex);
            return duplicate;
        }

        //public IByteBuf copy(int index, int length)
        //{
        //    CheckIndex(index, length);
        //    return this.buffer.Copy(index + this.adjustment, length);
        //}

        public override IByteBuf Copy(int index, int length)
        {
            CheckIndex(index, length);
            return _buffer.Copy(index + _adjustment, length);
        }

        public override IByteBuf Slice(int index, int length)
        {
            CheckIndex(index, length);
            if (length == 0)
            {
                return Unpooled.Empty;
            }
            return _buffer.Slice(index + _adjustment, length);
        }

        public override IByteBuf GetBytes(int index, IByteBuf dst, int dstIndex, int length)
        {
            CheckIndex(index, length);
            _buffer.GetBytes(index + _adjustment, dst, dstIndex, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            CheckIndex(index, length);
            _buffer.GetBytes(index + _adjustment, dst, dstIndex, length);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            return _buffer.SetByte(index + _adjustment, value);
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            return _buffer.SetShort(index + _adjustment, value);
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            return _buffer.SetInt(index + _adjustment, value);
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            return _buffer.SetLong(index + _adjustment, value);
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckIndex(index, length);
            _buffer.SetBytes(index + _adjustment, src, srcIndex, length);
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckIndex(index, length);
            _buffer.SetBytes(index + _adjustment, src, srcIndex, length);
            return this;
        }
    }
}