using System;
using System.Linq;
using Helios.Util;

namespace Helios.Buffers
{
    /// <summary>
    /// Concrete ByteBuffer implementation that uses a simple backing array
    /// </summary>
    public class ByteBuffer : ByteBufBase
    {
        protected byte[] Buffer;

        private int _capacity;

        public ByteBuffer(int initialCapacity, int maxCapacity) : base(maxCapacity)
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException("initialCapacity", "initialCapacity must be at least 0");
            if (maxCapacity < 0) throw new ArgumentOutOfRangeException("maxCapacity", "maxCapacity must be at least 0");
            if (initialCapacity > maxCapacity) throw new ArgumentException(string.Format("initialCapacity {0} must be less than maxCapacity {1}", initialCapacity, maxCapacity));
            Buffer = new byte[initialCapacity];
            _capacity = initialCapacity;
        }

        public override int Capacity
        {
            get { return _capacity; }
        }

        public override IByteBuf AdjustCapacity(int capacity)
        {
            if (capacity > MaxCapacity) throw new ArgumentOutOfRangeException("capacity", string.Format("capacity({0}) must be less than MaxCapacity({1})", capacity, MaxCapacity));
            var newBuffer = new byte[capacity];
            System.Array.Copy(Buffer, ReaderIndex, newBuffer,0, ReadableBytes);
            Buffer = newBuffer;
            _capacity = capacity;
            return this;
        }

        public override IByteBufAllocator Allocator
        {
            get { throw new NotImplementedException(); }
        }

        protected override byte _GetByte(int index)
        {
            return Buffer[index];
        }

        protected override short _GetShort(int index)
        {
            return BitConverter.ToInt16(Buffer.Slice(index,2), 0);
        }

        protected override int _GetInt(int index)
        {
            return BitConverter.ToInt32(Buffer.Slice(index,4), 0);
        }

        protected override long _GetLong(int index)
        {
            return BitConverter.ToInt64(Buffer.Slice(index, 8), 0);
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            CheckDstIndex(index,length, dstIndex, destination.WritableBytes);
            destination.SetBytes(dstIndex, Buffer.Slice(index, length), 0, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.Length);
            System.Array.Copy(Buffer, index, destination, dstIndex, length);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            Buffer.SetValue((byte)value,index);
            return this;
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            Buffer.SetRange(index, BitConverter.GetBytes(Convert.ToInt16(value)));
            return this;
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            Buffer.SetRange(index, BitConverter.GetBytes(value));
            return this;
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            Buffer.SetRange(index, BitConverter.GetBytes(value));
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.ReadableBytes);
            if (src.HasArray)
            {
                Buffer.SetRange(index, src.InternalArray().Slice(srcIndex, length));
            }
            else
            {
                src.ReadBytes(Buffer, WriterIndex, length);
            }
            return this;
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            System.Array.Copy(src, srcIndex, Buffer, WriterIndex, length);
            return this;
        }

        public override bool HasArray
        {
            get { return true; }
        }

        public override byte[] InternalArray()
        {
            return Buffer;
        }
    }
}