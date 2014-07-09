using System;
using Helios.Util;
using Helios.Util.Collections;

namespace Helios.Buffers
{
    /// <summary>
    /// Circular implementation of a ByteBuf - used in scenarios
    /// where you don't care about manual indexing
    /// </summary>
    public class CircularByteBuf : AbstractByteBuf
    {
        protected ICircularBuffer<byte> InternalBuffer;
        protected IByteBufAllocator Alloc;

        public static CircularByteBuf AllocateDirect(int capacity, int maxCapacity = Int32.MaxValue)
        {
            return new CircularByteBuf(capacity, maxCapacity);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        internal protected CircularByteBuf(IByteBufAllocator allocator, ICircularBuffer<byte> buffer) : base(buffer.MaxCapacity)
        {
            InternalBuffer = buffer;
            Alloc = allocator;
        }

        internal protected CircularByteBuf(int initialCapacity, int maxCapacity)
            : this(null, new CircularBuffer<byte>(initialCapacity, maxCapacity))
        {
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException("initialCapacity", "initialCapacity must be at least 0");
            if (maxCapacity < 0) throw new ArgumentOutOfRangeException("maxCapacity", "maxCapacity must be at least 0");
            if (initialCapacity > maxCapacity) throw new ArgumentException(string.Format("initialCapacity {0} must be less than maxCapacity {1}", initialCapacity, maxCapacity));
        }

        public override int Capacity { get { return InternalBuffer.Capacity; } }
        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            InternalBuffer.Capacity = newCapacity;
            return this;
        }

        public override IByteBufAllocator Allocator { get { return Alloc; } }
        public override int ReaderIndex { get { return InternalBuffer.Head; } }
        public override int WriterIndex { get { return InternalBuffer.Tail; } }
        public override IByteBuf SetWriterIndex(int writerIndex)
        {
            InternalBuffer.SetTail(writerIndex);
            return this;
        }

        public override IByteBuf SetReaderIndex(int readerIndex)
        {
            InternalBuffer.SetHead(readerIndex);
            return this;
        }

        public override IByteBuf SetIndex(int readerIndex, int writerIndex)
        {
            SetReaderIndex(readerIndex);
            SetWriterIndex(writerIndex);
            return this;
        }

        public override int ReadableBytes { get { return InternalBuffer.Size; } }
        public override int WritableBytes { get { return InternalBuffer.Capacity - InternalBuffer.Size; } }
        public override int MaxWritableBytes { get { return InternalBuffer.MaxCapacity - InternalBuffer.Size; } }

        public override IByteBuf Clear()
        {
            InternalBuffer.Clear();
            return this;
        }

        protected override byte _GetByte(int index)
        {
            return InternalBuffer[index];
        }

        protected override short _GetShort(int index)
        {
            return BitConverter.ToInt16(InternalBuffer.Slice(index, 2), 0);
        }

        protected override int _GetInt(int index)
        {
            return BitConverter.ToInt32(InternalBuffer.Slice(index, 4), 0);
        }

        protected override long _GetLong(int index)
        {
            return BitConverter.ToInt64(InternalBuffer.Slice(index, 8), 0);
        }

        
        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.WritableBytes);
            destination.SetBytes(dstIndex, InternalBuffer.Slice(index, length), 0, length);
            return this;
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            CheckDstIndex(index, length, dstIndex, destination.Length);
            InternalBuffer.DirectBufferRead(index, destination, dstIndex, index);
            return this;
        }

        public override IByteBuf ReadBytes(int length)
        {
            CheckReadableBytes(length);
            if (length == 0) return Unpooled.Empty;

            var buf = new byte[length];
            InternalBuffer.DirectBufferRead(buf, 0, length);
            return new ByteBuffer(buf, length, length).SetWriterIndex(length);
        }

        public override byte ReadByte()
        {
            CheckReadableBytes(1);
            return InternalBuffer.Dequeue();
        }

        public override short ReadShort()
        {
            CheckReadableBytes(2);
            return BitConverter.ToInt16(InternalBuffer.Slice(2),0);
        }

        public override int ReadInt()
        {
            CheckReadableBytes(4);
            return BitConverter.ToInt32(InternalBuffer.Slice(4),0);
        }

        public override long ReadLong()
        {
            CheckReadableBytes(8);
            return BitConverter.ToInt64(InternalBuffer.Slice(8), 0);
        }

        public override IByteBuf ReadBytes(IByteBuf destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            destination.SetBytes(dstIndex, InternalBuffer.Slice(length));
            return this;
        }

        public override IByteBuf ReadBytes(byte[] destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            InternalBuffer.DirectBufferRead(destination, dstIndex, length);
            return this;
        }

        public override IByteBuf SkipBytes(int length)
        {
            CheckReadableBytes(length);
            InternalBuffer.Skip(length);
            return this;
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            InternalBuffer[index] = (byte) value;
            return this;
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            unchecked
            {
                InternalBuffer.SetRange(index, BitConverter.GetBytes((short)(value)));
            }
            return this;
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            InternalBuffer.SetRange(index, BitConverter.GetBytes(value));
            return this;
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            InternalBuffer.SetRange(index, BitConverter.GetBytes(value));
            return this;
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.ReadableBytes);
            if (src.HasArray)
            {
                InternalBuffer.SetRange(index, src.InternalArray().Slice(srcIndex, length));
            }
            else
            {
                var tempBuffer = new byte[length];
                src.ReadBytes(tempBuffer, srcIndex, length);
                InternalBuffer.SetRange(index, tempBuffer);
            }
            return this;
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            CheckSrcIndex(index, length, srcIndex, src.Length);
            InternalBuffer.DirectBufferWrite(index, src, srcIndex, length);
            return this;
        }

        public override IByteBuf WriteByte(int value)
        {
            EnsureWritable(1);
            InternalBuffer.Enqueue((byte)value);
            return this;
        }

        public override IByteBuf WriteShort(int value)
        {
            EnsureWritable(2);
            InternalBuffer.DirectBufferWrite(BitConverter.GetBytes((short)value));
            return this;
        }

        public override IByteBuf WriteInt(int value)
        {
            EnsureWritable(4);
            InternalBuffer.DirectBufferWrite(BitConverter.GetBytes(value));
            return this;
        }

        public override IByteBuf WriteLong(long value)
        {
            EnsureWritable(8);
            InternalBuffer.DirectBufferWrite(BitConverter.GetBytes(value));
            return this;
        }

        public override IByteBuf WriteBytes(IByteBuf src, int srcIndex, int length)
        {
            EnsureWritable(length);
            if (src.HasArray)
            {
                InternalBuffer.DirectBufferWrite(src.InternalArray().Slice(srcIndex, length));
                src.SetReaderIndex(src.ReaderIndex + length);
            }
            else
            {
                var tempBuffer = new byte[length];
                src.ReadBytes(tempBuffer, srcIndex, length);
                InternalBuffer.DirectBufferWrite(tempBuffer);
            }
            return this;
        }

        public override IByteBuf WriteBytes(byte[] src, int srcIndex, int length)
        {
            EnsureWritable(length);
            InternalBuffer.DirectBufferWrite(src, srcIndex, length);
            return this;
        }
        
        public override bool HasArray
        {
            get { return false; }
        }

        public override byte[] InternalArray()
        {
            throw new NotSupportedException();
        }

        public override bool IsDirect
        {
            get { return true; }
        }

        public override IByteBuf Unwrap()
        {
            return null;
        }

        public override byte[] ToArray()
        {
            return InternalBuffer.ToArray();
        }

        public override ByteBuffer InternalNioBuffer(int index, int length)
        {
            return (ByteBuffer)(Duplicate()).Clear().SetIndex(index, length);
        }

        public override IByteBuf Compact()
        {
            return this;
        }

        public override IByteBuf CompactIfNecessary()
        {
            return this;
        }
    }
}
