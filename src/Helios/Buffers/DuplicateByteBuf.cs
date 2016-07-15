// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Buffers
{
    /// <summary>
    ///     Derived buffer that forwards requests to the original underlying buffer
    /// </summary>
    public class DuplicateByteBuf : AbstractDerivedByteBuffer
    {
        private readonly IByteBuf _buffer;

        public DuplicateByteBuf(IByteBuf source)
            : base(source.MaxCapacity)
        {
            var dupe = source as DuplicateByteBuf;
            _buffer = dupe != null ? dupe._buffer : source;
            SetIndex(source.ReaderIndex, source.WriterIndex);
        }

        public override int Capacity
        {
            get { return _buffer.Capacity; }
        }

        public override ByteOrder Order => _buffer.Order;

        public override IByteBufAllocator Allocator
        {
            get { return _buffer.Allocator; }
        }

        public override bool HasArray
        {
            get { return _buffer.HasArray; }
        }

        public override byte[] Array
        {
            get { return _buffer.Array; }
        }

        public override bool IsDirect
        {
            get { return _buffer.IsDirect; }
        }

        public override int ArrayOffset => _buffer.ArrayOffset;

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            return _buffer.AdjustCapacity(newCapacity);
        }

        public override byte GetByte(int index)
        {
            return _GetByte(index);
        }

        protected override byte _GetByte(int index)
        {
            return _buffer.GetByte(index);
        }

        public override short GetShort(int index)
        {
            return _GetShort(index);
        }

        protected override short _GetShort(int index)
        {
            return _buffer.GetShort(index);
        }

        public override int GetInt(int index)
        {
            return _GetInt(index);
        }

        protected override int _GetInt(int index)
        {
            return _buffer.GetInt(index);
        }

        public override long GetLong(int index)
        {
            return _GetLong(index);
        }

        protected override long _GetLong(int index)
        {
            return _buffer.GetLong(index);
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            _buffer.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            _buffer.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination)
        {
            _buffer.GetBytes(index, destination);
            return this;
        }

        public override IByteBuf SetByte(int index, int value)
        {
            _SetByte(index, value);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            _buffer.SetByte(index, value);
            return this;
        }

        public override IByteBuf SetShort(int index, int value)
        {
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
            _SetInt(index, value);
            return this;
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            _buffer.SetInt(index, value);
            return this;
        }

        public override IByteBuf SetLong(int index, long value)
        {
            _SetLong(index, value);
            return this;
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            _buffer.SetLong(index, value);
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            _buffer.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            _buffer.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public override IByteBuf Copy(int index, int length)
        {
            return _buffer.Copy(index, length);
        }

        public override IByteBuf Unwrap()
        {
            return _buffer;
        }

        public override int IoBufferCount => Unwrap().IoBufferCount;

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length) => Unwrap().GetIoBuffers(index, length);

        public override IByteBuf Compact()
        {
            _buffer.Compact();
            SetIndex(_buffer.ReaderIndex, _buffer.WriterIndex);
            return this;
        }

        public override IByteBuf CompactIfNecessary()
        {
            _buffer.CompactIfNecessary();
            SetIndex(_buffer.ReaderIndex, _buffer.WriterIndex);
            return this;
        }
    }
}