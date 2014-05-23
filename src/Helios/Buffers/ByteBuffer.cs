using System;
using Helios.Util;

namespace Helios.Buffers
{
    /// <summary>
    /// Concrete ByteBuffer implementation that uses a simple backing array
    /// </summary>
    public class ByteBuffer : AbstractByteBuf
    {
        protected byte[] Buffer;

        private int _capacity;

        public static ByteBuffer AllocateDirect(int capacity, int maxCapacity = ByteBufferUtil.DEFAULT_MAX_CAPACITY)
        {
            return new ByteBuffer(capacity, maxCapacity);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        internal protected ByteBuffer(byte[] buffer, int initialCapacity, int maxCapacity)
            : base(maxCapacity)
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException("initialCapacity", "initialCapacity must be at least 0");
            if (maxCapacity < 0) throw new ArgumentOutOfRangeException("maxCapacity", "maxCapacity must be at least 0");
            if (initialCapacity > maxCapacity) throw new ArgumentException(string.Format("initialCapacity {0} must be less than maxCapacity {1}", initialCapacity, maxCapacity));
            Buffer = buffer;
            _capacity = initialCapacity;
        }

        protected ByteBuffer(int initialCapacity, int maxCapacity)
            : this(new byte[initialCapacity], initialCapacity, maxCapacity)
        {
        }

        public override int Capacity
        {
            get { return _capacity; }
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            if (newCapacity > MaxCapacity) throw new ArgumentOutOfRangeException("newCapacity", string.Format("capacity({0}) must be less than MaxCapacity({1})", newCapacity, MaxCapacity));
            var newBuffer = new byte[newCapacity];

            //expand
            if (newCapacity > Capacity)
            {
                Array.Copy(Buffer, ReaderIndex, newBuffer, 0, ReadableBytes);
                SetIndex(0, ReadableBytes);
            }
            else //shrink
            {
                Array.Copy(Buffer, ReaderIndex, newBuffer, 0, newCapacity);
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

            Buffer = newBuffer;
            _capacity = newCapacity;
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
            return BitConverter.ToInt16(Buffer.Slice(index, 2), 0);
        }

        protected override int _GetInt(int index)
        {
            return BitConverter.ToInt32(Buffer.Slice(index, 4), 0);
        }

        protected override long _GetLong(int index)
        {
            return BitConverter.ToInt64(Buffer.Slice(index, 8), 0);
        }

        public override IByteBuf ReadBytes(int length)
        {
            CheckReadableBytes(length);
            if (length == 0) return Unpooled.Empty;

            var buf = new byte[length];
            Array.Copy(Buffer, ReaderIndex, buf, 0, length);
            ReaderIndex += length;
            return new ByteBuffer(buf, length, length).SetWriterIndex(length);
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.WritableBytes);
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
            Buffer.SetValue((byte)value, index);
            return this;
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            unchecked
            {
                Buffer.SetRange(index, BitConverter.GetBytes((short)(value)));
            }
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
                src.ReadBytes(Buffer, srcIndex, length);
            }
            return this;
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            System.Array.Copy(src, srcIndex, Buffer, index, length);
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

        public override bool IsDirect
        {
            get { return true; }
        }

        public override IByteBuf Unwrap()
        {
            return null;
        }

        public override ByteBuffer InternalNioBuffer(int index, int length)
        {
            return (ByteBuffer)(Duplicate()).Clear().SetIndex(index, length);
        }

        public override IByteBuf Compact()
        {
            var buffer = new byte[Capacity];
            Array.Copy(Buffer, ReaderIndex, buffer, 0, ReadableBytes);
            Buffer = buffer;
            SetIndex(0, ReadableBytes);
            return this;
        }

        /// <summary>
        /// Duplicate for <see cref="ByteBuffer"/> instances actually creates a deep clone, rather than a proxy
        /// </summary>
        public IByteBuf DeepDuplicate()
        {
            var buffer = new byte[Capacity];
            Array.Copy(Buffer, ReaderIndex, buffer, 0, ReadableBytes);
            return new ByteBuffer(buffer, Capacity, MaxCapacity).SetIndex(ReaderIndex, WriterIndex);
        }
    }
}