// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Buffers
{
    public sealed class SlicedByteBuffer : AbstractDerivedByteBuffer
    {
        private readonly int adjustment;
        private readonly IByteBuf buffer;
        private readonly int length;

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
                this.buffer = slicedByteBuf.buffer;
                adjustment = slicedByteBuf.adjustment + index;
            }
            else if (buffer is DuplicateByteBuf)
            {
                this.buffer = buffer.Unwrap();
                adjustment = index;
            }
            else
            {
                this.buffer = buffer;
                adjustment = index;
            }
            this.length = length;

            SetWriterIndex(length);
        }

        public override IByteBufAllocator Allocator
        {
            get { return buffer.Allocator; }
        }

        public override ByteOrder Order
        {
            get { return buffer.Order; }
        }

        public override int Capacity
        {
            get { return length; }
        }

        public override bool HasArray
        {
            get { return buffer.HasArray; }
        }

        public override byte[] Array
        {
            get { return buffer.Array; }
        }

        public override bool IsDirect => buffer.IsDirect;

        public override int ArrayOffset
        {
            get { return buffer.ArrayOffset + adjustment; }
        }

        public override IByteBuf Unwrap()
        {
            return buffer;
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
            return buffer.GetByte(index + adjustment);
        }

        protected override short _GetShort(int index)
        {
            return buffer.GetShort(index + adjustment);
        }

        protected override int _GetInt(int index)
        {
            return buffer.GetInt(index + adjustment);
        }

        protected override long _GetLong(int index)
        {
            return buffer.GetLong(index + adjustment);
        }

        public override IByteBuf Duplicate()
        {
            var duplicate = buffer.Slice(adjustment, length);
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
            return buffer.Copy(index + adjustment, length);
        }

        public override IByteBuf Slice(int index, int length)
        {
            CheckIndex(index, length);
            if (length == 0)
            {
                return Unpooled.Empty;
            }
            return buffer.Slice(index + adjustment, length);
        }

        public override IByteBuf GetBytes(int index, IByteBuf dst, int dstIndex, int length)
        {
            CheckIndex(index, length);
            buffer.GetBytes(index + adjustment, dst, dstIndex, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            CheckIndex(index, length);
            buffer.GetBytes(index + adjustment, dst, dstIndex, length);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            return buffer.SetByte(index + adjustment, value);
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            return buffer.SetShort(index + adjustment, value);
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            return buffer.SetInt(index + adjustment, value);
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            return buffer.SetLong(index + adjustment, value);
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckIndex(index, length);
            buffer.SetBytes(index + adjustment, src, srcIndex, length);
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckIndex(index, length);
            buffer.SetBytes(index + adjustment, src, srcIndex, length);
            return this;
        }
    }
}

