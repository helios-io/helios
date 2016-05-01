using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Buffers
{
    public sealed class SlicedByteBuffer : AbstractDerivedByteBuffer
    {
        private readonly IByteBuf buffer;
        private readonly int adjustment;
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
                this.adjustment = slicedByteBuf.adjustment + index;
            }
            else if (buffer is DuplicateByteBuf)
            {
                this.buffer = buffer.Unwrap();
                this.adjustment = index;
            }
            else
            {
                this.buffer = buffer;
                this.adjustment = index;
            }
            this.length = length;

            this.SetWriterIndex(length);
        }

        public override IByteBuf Unwrap()
        {
            return this.buffer;
        }

        public override IByteBuf Compact()
        {
            throw new NotImplementedException();
        }

        public override IByteBuf CompactIfNecessary()
        {
            throw new NotImplementedException();
        }

        public override IByteBufAllocator Allocator
        {
            get { return this.buffer.Allocator; }
        }

        public override ByteOrder Order
        {
            get { return this.buffer.Order; }
        }

        public override int Capacity
        {
            get { return this.length; }
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            throw new NotSupportedException("sliced buffer");
        }

        public override bool HasArray
        {
            get { return this.buffer.HasArray; }
        }

        public override byte[] Array
        {
            get { return buffer.Array; }
        }

        public override bool IsDirect => buffer.IsDirect;

        public override int ArrayOffset
        {
            get { return this.buffer.ArrayOffset + this.adjustment; }
        }

        protected override byte _GetByte(int index)
        {
            return this.buffer.GetByte(index + this.adjustment);
        }

        protected override short _GetShort(int index)
        {
            return this.buffer.GetShort(index + this.adjustment);
        }

        protected override int _GetInt(int index)
        {
            return this.buffer.GetInt(index + this.adjustment);
        }

        protected override long _GetLong(int index)
        {
            return this.buffer.GetLong(index + this.adjustment);
        }

        public override IByteBuf Duplicate()
        {
            IByteBuf duplicate = this.buffer.Slice(this.adjustment, this.length);
            duplicate.SetIndex(this.ReaderIndex, this.WriterIndex);
            return duplicate;
        }

        //public IByteBuf copy(int index, int length)
        //{
        //    CheckIndex(index, length);
        //    return this.buffer.Copy(index + this.adjustment, length);
        //}

        public override IByteBuf Copy(int index, int length)
        {
            this.CheckIndex(index, length);
            return this.buffer.Copy(index + this.adjustment, length);
        }

        public override IByteBuf Slice(int index, int length)
        {
            this.CheckIndex(index, length);
            if (length == 0)
            {
                return Unpooled.Empty;
            }
            return this.buffer.Slice(index + this.adjustment, length);
        }

        public override IByteBuf GetBytes(int index, IByteBuf dst, int dstIndex, int length)
        {
            this.CheckIndex(index, length);
            this.buffer.GetBytes(index + this.adjustment, dst, dstIndex, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] dst, int dstIndex, int length)
        {
            this.CheckIndex(index, length);
            this.buffer.GetBytes(index + this.adjustment, dst, dstIndex, length);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            return buffer.SetByte(index + this.adjustment, value);
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            return buffer.SetShort(index + this.adjustment, value);
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            return buffer.SetInt(index + this.adjustment, value);
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            return buffer.SetLong(index + this.adjustment, value);
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            this.CheckIndex(index, length);
            this.buffer.SetBytes(index + this.adjustment, src, srcIndex, length);
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            this.CheckIndex(index, length);
            this.buffer.SetBytes(index + this.adjustment, src, srcIndex, length);
            return this;
        }
    }
}
