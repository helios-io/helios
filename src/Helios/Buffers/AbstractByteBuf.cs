using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Util;

namespace Helios.Buffers
{
    /// <summary>
    /// Abstract base class implementation of a <see cref="IByteBuf"/>
    /// </summary>
    public abstract class AbstractByteBuf : IByteBuf
    {
        private int _markedReaderIndex;
        private int _markedWriterIndex;

        protected AbstractByteBuf(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
        }

        public abstract int Capacity { get; }

        public abstract IByteBuf AdjustCapacity(int newCapacity);
        public abstract ByteOrder Endianness { get; }

        public int MaxCapacity { get; private set; }
        public abstract IByteBufAllocator Allocator { get; }
        public virtual int ReaderIndex { get; protected set; }
        public virtual int WriterIndex { get; protected set; }
        public virtual IByteBuf SetWriterIndex(int writerIndex)
        {
            if (writerIndex < ReaderIndex || writerIndex > Capacity)
                throw new IndexOutOfRangeException(string.Format("WriterIndex: {0} (expected: 0 <= readerIndex({1}) <= writerIndex <= capacity ({2})", writerIndex, ReaderIndex, Capacity));

            WriterIndex = writerIndex;
            return this;
        }

        public virtual IByteBuf SetReaderIndex(int readerIndex)
        {
            if (readerIndex < 0 || readerIndex > WriterIndex)
                throw new IndexOutOfRangeException(string.Format("ReaderIndex: {0} (expected: 0 <= readerIndex <= writerIndex({1})", readerIndex, WriterIndex));
            ReaderIndex = readerIndex;
            return this;
        }

        public virtual IByteBuf SetIndex(int readerIndex, int writerIndex)
        {
            if (readerIndex < 0 || readerIndex > writerIndex || writerIndex > Capacity)
                throw new IndexOutOfRangeException(string.Format("ReaderIndex: {0}, WriterIndex: {1} (expected: 0 <= readerIndex <= writerIndex <= capacity ({2})", readerIndex, writerIndex, Capacity));

            ReaderIndex = readerIndex;
            WriterIndex = writerIndex;
            return this;
        }

        public virtual int ReadableBytes { get { return WriterIndex - ReaderIndex; } }
        public virtual int WritableBytes { get { return Capacity - WriterIndex; } }

        public virtual int MaxWritableBytes
        {
            get { return MaxCapacity - WriterIndex; }
        }

        public bool IsReadable()
        {
            return WriterIndex > ReaderIndex;
        }

        public bool IsReadable(int size)
        {
            return WriterIndex - ReaderIndex >= size;
        }

        public bool IsWritable()
        {
            return Capacity > WriterIndex;
        }

        public bool IsWritable(int size)
        {
            return Capacity - WriterIndex >= size;
        }

        public virtual IByteBuf Clear()
        {
            ReaderIndex = WriterIndex = 0;
            return this;
        }

        public virtual IByteBuf MarkReaderIndex()
        {
            _markedReaderIndex = ReaderIndex;
            return this;
        }

        public virtual IByteBuf ResetReaderIndex()
        {
            SetReaderIndex(_markedReaderIndex);
            return this;
        }

        public virtual IByteBuf MarkWriterIndex()
        {
            _markedWriterIndex = WriterIndex;
            return this;
        }

        public virtual IByteBuf ResetWriterIndex()
        {
            SetWriterIndex(_markedWriterIndex);
            return this;
        }

        public virtual IByteBuf DiscardReadBytes()
        {
            EnsureAccessible();
            if (ReaderIndex == 0) return this;

            if (ReaderIndex != WriterIndex)
            {
                SetBytes(0, this, ReaderIndex, WriterIndex - ReaderIndex);
                WriterIndex -= ReaderIndex;
                AdjustMarkers(ReaderIndex);
                ReaderIndex = 0;
            }
            else
            {
                AdjustMarkers(ReaderIndex);
                WriterIndex = ReaderIndex = 0;
            }

            return this;
        }

        public virtual IByteBuf DiscardSomeReadBytes()
        {
            EnsureAccessible();
            if (ReaderIndex == 0) return this;

            if (ReaderIndex == WriterIndex) // everything has been read
            {
                AdjustMarkers(ReaderIndex);
                WriterIndex = ReaderIndex = 0;
                return this;
            }

            unchecked
            {
                if (ReaderIndex >= Capacity >> 1)
                {
                    SetBytes(0, this, ReaderIndex, WriterIndex - ReaderIndex);
                    WriterIndex -= ReaderIndex;
                    AdjustMarkers(ReaderIndex);
                    ReaderIndex = 0;
                }
            }

            return this;
        }

        public virtual IByteBuf EnsureWritable(int minWritableBytes)
        {
            if (minWritableBytes < 0)
                throw new ArgumentOutOfRangeException("minWritableBytes",
                    "expected minWritableBytes to be greater than zero");

            if (minWritableBytes <= WritableBytes) return this;

            if (minWritableBytes > MaxCapacity - WriterIndex)
            {
                throw new IndexOutOfRangeException(string.Format(
                    "writerIndex({0}) + minWritableBytes({1}) exceeds maxCapacity({2}): {3}",
                    WriterIndex, minWritableBytes, MaxCapacity, this));
            }

            //Normalize the current capacity to the power of 2
            var newCapacity = CalculateNewCapacity(WriterIndex + minWritableBytes);

            //Adjust to the new capacity
            AdjustCapacity(newCapacity);
            return this;
        }

        private int CalculateNewCapacity(int minNewCapacity)
        {
            var maxCapacity = MaxCapacity;
            var threshold = 1048576 * 4; // 4 MiB page
            var newCapacity = 0;
            if (minNewCapacity == threshold)
            {
                return threshold;
            }

            // If over threshold, do not double but just increase by threshold.
            if (minNewCapacity > threshold)
            {
                newCapacity = minNewCapacity / threshold * threshold;
                if (newCapacity > maxCapacity - threshold)
                {
                    newCapacity = maxCapacity;
                }
                else
                {
                    newCapacity += threshold;
                }
                return newCapacity;
            }

            // Not over threshold. Double up to 4 MiB, starting from 64.
            newCapacity = 64;
            while (newCapacity < minNewCapacity)
            {
                newCapacity <<= 1;
            }

            return Math.Min(newCapacity, maxCapacity);
        }

        public virtual bool GetBoolean(int index)
        {
            CheckIndex(index);
            return GetByte(index) != 0;
        }

        public virtual byte GetByte(int index)
        {
            CheckIndex(index);
            return _GetByte(index);
        }

        protected abstract byte _GetByte(int index);

        public virtual short GetShort(int index)
        {
            CheckIndex(index, 2);
            return _GetShort(index);
        }

        protected abstract short _GetShort(int index);

        public virtual ushort GetUnsignedShort(int index)
        {
            unchecked
            {
                return (ushort)(GetShort(index));
            }
            
        }

        public virtual int GetInt(int index)
        {
            CheckIndex(index, 4);
            return _GetInt(index);
        }

        protected abstract int _GetInt(int index);

        public virtual uint GetUnsignedInt(int index)
        {
            unchecked
            {
                return (uint)(GetInt(index));
            }
        }

        public virtual long GetLong(int index)
        {
            CheckIndex(index, 8);
            return _GetLong(index);
        }

        protected abstract long _GetLong(int index);

        public virtual char GetChar(int index)
        {
            return Convert.ToChar(GetShort(index));
        }

        public virtual double GetDouble(int index)
        {
            return BitConverter.Int64BitsToDouble(GetLong(index));
        }

        public virtual IByteBuf GetBytes(int index, IByteBuf destination)
        {
            GetBytes(index, destination, destination.WritableBytes);
            return this;
        }

        public virtual IByteBuf GetBytes(int index, IByteBuf destination, int length)
        {
            GetBytes(index, destination, destination.WriterIndex, length);
            return this;
        }

        public abstract IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length);

        public virtual IByteBuf GetBytes(int index, byte[] destination)
        {
            GetBytes(index, destination, 0, destination.Length);
            return this;
        }

        public abstract IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length);

        public virtual IByteBuf SetBoolean(int index, bool value)
        {
            SetByte(index, value ? 1 : 0);
            return this;
        }

        public virtual IByteBuf SetByte(int index, int value)
        {
            CheckIndex(index);
            _SetByte(index, value);
            return this;
        }

        protected abstract IByteBuf _SetByte(int index, int value);

        public virtual IByteBuf SetShort(int index, int value)
        {
            CheckIndex(index, 2);
            _SetShort(index, value);
            return this;
        }

        public IByteBuf SetUnsignedShort(int index, int value)
        {
            SetShort(index, value);
            return this;
        }

        protected abstract IByteBuf _SetShort(int index, int value);

        public virtual IByteBuf SetInt(int index, int value)
        {
            CheckIndex(index, 4);
            _SetInt(index, value);
            return this;
        }

        public IByteBuf SetUnsignedInt(int index, uint value)
        {
            unchecked
            {
                SetInt(index, (int)value);
            }
            return this;
        }

        protected abstract IByteBuf _SetInt(int index, int value);

        public virtual IByteBuf SetLong(int index, long value)
        {
            CheckIndex(index, 8);
            _SetLong(index, value);
            return this;
        }

        protected abstract IByteBuf _SetLong(int index, long value);

        public virtual IByteBuf SetChar(int index, char value)
        {
            SetShort(index, value);
            return this;
        }

        public virtual IByteBuf SetDouble(int index, double value)
        {
            SetLong(index, BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public virtual IByteBuf SetBytes(int index, IByteBuf src)
        {
            SetBytes(index, src, src.ReadableBytes);
            return this;
        }

        public virtual IByteBuf SetBytes(int index, IByteBuf src, int length)
        {
            CheckIndex(index, length);
            if(src == null) throw new NullReferenceException("src cannot be null");
            if (length > src.ReadableBytes) throw new IndexOutOfRangeException(string.Format(
                     "length({0}) exceeds src.readableBytes({1}) where src is: {2}", length, src.ReadableBytes, src));
            SetBytes(index, src, src.ReaderIndex, length);
            src.SetReaderIndex(src.ReaderIndex + length);
            return this;
        }

        public abstract IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length);

        public virtual IByteBuf SetBytes(int index, byte[] src)
        {
            SetBytes(index, src, 0, src.Length);
            return this;
        }

        public abstract IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length);

        public virtual bool ReadBoolean()
        {
            return ReadByte() != 0;
        }

        public virtual byte ReadByte()
        {
            CheckReadableBytes(1);
            var i = ReaderIndex;
            var b = GetByte(i);
            ReaderIndex = i + 1;
            return b;
        }

        public virtual short ReadShort()
        {
            CheckReadableBytes(2);
            var v = _GetShort(ReaderIndex);
            ReaderIndex += 2;
            return v;
        }

        public virtual ushort ReadUnsignedShort()
        {
            unchecked
            {
                return (ushort)(ReadShort());
            }
        }

        public virtual int ReadInt()
        {
            CheckReadableBytes(4);
            var v = _GetInt(ReaderIndex);
            ReaderIndex += 4;
            return v;
        }

        public virtual uint ReadUnsignedInt()
        {
            unchecked
            {
                return (uint)(ReadInt());
            }
        }

        public virtual long ReadLong()
        {
            CheckReadableBytes(8);
            var v = _GetLong(ReaderIndex);
            ReaderIndex += 8;
            return v;
        }

        public virtual char ReadChar()
        {
            return (char) ReadShort();
        }

        public virtual double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        public virtual IByteBuf ReadBytes(int length)
        {
            CheckReadableBytes(length);
            if (length == 0) return Unpooled.Empty;

            var buf = Unpooled.Buffer(length, MaxCapacity);
            buf.WriteBytes(this, ReaderIndex, length);
            ReaderIndex += length;
            return buf;
        }

        public virtual IByteBuf ReadBytes(IByteBuf destination)
        {
            ReadBytes(destination, destination.WritableBytes);
            return this;
        }

        public virtual IByteBuf ReadBytes(IByteBuf destination, int length)
        {
            if(length > destination.WritableBytes) 
                throw new IndexOutOfRangeException(string.Format("length({0}) exceeds destination.WritableBytes({1}) where destination is: {2}", 
                    length, destination.WritableBytes, destination));
            ReadBytes(destination, destination.WriterIndex, length);
            destination.SetWriterIndex(destination.WriterIndex + length);
            return this;
        }

        public virtual IByteBuf ReadBytes(IByteBuf destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            GetBytes(ReaderIndex, destination, dstIndex, length);
            ReaderIndex += length;
            return this;
        }

        public virtual IByteBuf ReadBytes(byte[] destination)
        {
            ReadBytes(destination, 0, destination.Length);
            return this;
        }

        public virtual IByteBuf ReadBytes(byte[] destination, int dstIndex, int length)
        {
            CheckReadableBytes(length);
            GetBytes(ReaderIndex, destination, dstIndex, length);
            ReaderIndex += length;
            return this;
        }

        public virtual IByteBuf SkipBytes(int length)
        {
            CheckReadableBytes(length);
            var newReaderIndex = ReaderIndex + length;
            if(newReaderIndex > WriterIndex)
                throw new IndexOutOfRangeException(string.Format(
                    "length: {0} (expected: readerIndex({1}) + length <= writerIndex({2}))",
                    length, ReaderIndex, WriterIndex));
            ReaderIndex = newReaderIndex;
            return this;
        }

        public virtual IByteBuf WriteBoolean(bool value)
        {
            WriteByte(value ? 1 : 0);
            return this;
        }

        public virtual IByteBuf WriteByte(int value)
        {
            EnsureWritable(1);
            SetByte(WriterIndex, value);
            WriterIndex += 1;
            return this;
        }

        public virtual IByteBuf WriteShort(int value)
        {
            EnsureWritable(2);
            _SetShort(WriterIndex, value);
            WriterIndex += 2;
            return this;
        }

        public IByteBuf WriteUnsignedShort(int value)
        {
            unchecked
            {
                WriteShort((ushort) value);
            }
            return this;
        }

        public virtual IByteBuf WriteInt(int value)
        {
            EnsureWritable(4);
            _SetInt(WriterIndex, value);
            WriterIndex += 4;
            return this;
        }

        public IByteBuf WriteUnsignedInt(uint value)
        {
            unchecked
            {
                WriteInt((int) value);
            }
            return this;
        }

        public virtual IByteBuf WriteLong(long value)
        {
            EnsureWritable(8);
            _SetLong(WriterIndex, value);
            WriterIndex += 8;
            return this;
        }

        public virtual IByteBuf WriteChar(char value)
        {
            WriteShort(value);
            return this;
        }

        public virtual IByteBuf WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public virtual IByteBuf WriteBytes(IByteBuf src)
        {
            WriteBytes(src, src.ReadableBytes);
            return this;
        }

        public virtual IByteBuf WriteBytes(IByteBuf src, int length)
        {
            if (length > src.ReadableBytes)
                throw new IndexOutOfRangeException(string.Format("length({0}) exceeds src.readableBytes({1}) where src is: {2}", length, src.ReadableBytes, src));
            WriteBytes(src, src.ReaderIndex, length);
            src.SetReaderIndex(src.ReaderIndex + length);
            return this;
        }

        public virtual IByteBuf WriteBytes(IByteBuf src, int srcIndex, int length)
        {
            EnsureWritable(length);
            SetBytes(WriterIndex, src, srcIndex, length);
            WriterIndex += length;
            return this;
        }

        public virtual IByteBuf WriteBytes(byte[] src)
        {
            WriteBytes(src, 0, src.Length);
            return this;
        }

        public virtual IByteBuf WriteBytes(byte[] src, int srcIndex, int length)
        {
            EnsureWritable(length);
            SetBytes(WriterIndex, src, srcIndex, length);
            WriterIndex += length;
            return this;
        }

        public abstract bool HasArray { get; }
        public abstract byte[] InternalArray();
        public virtual byte[] ToArray()
        {
            if (HasArray)
            {
                return InternalArray().Slice(ReaderIndex, ReadableBytes);
            }

            var bytes = new byte[ReadableBytes];
            GetBytes(ReaderIndex, bytes);
            return bytes;
        }

        public abstract bool IsDirect { get; }

        public IByteBuf Copy()
        {
            return Copy(ReaderIndex, ReadableBytes);
        }
        public abstract IByteBuf Copy(int index, int length);

        public virtual IByteBuf Duplicate()
        {
            return new DuplicateByteBuf(this);
        }

        public abstract IByteBuf Unwrap();
        public abstract ByteBuffer InternalNioBuffer(int index, int length);
        public abstract IByteBuf Compact();
        public abstract IByteBuf CompactIfNecessary();

        protected void AdjustMarkers(int decrement)
        {
            var markedReaderIndex = _markedReaderIndex;
            if (markedReaderIndex <= decrement)
            {
                _markedReaderIndex = 0;
                var markedWriterIndex = _markedWriterIndex;
                if (markedWriterIndex <= decrement)
                {
                    _markedWriterIndex = 0;
                }
                else
                {
                    _markedWriterIndex = markedWriterIndex - decrement;
                }
            }
            else
            {
                _markedReaderIndex = markedReaderIndex - decrement;
                _markedWriterIndex -= decrement;
            }
        }

        protected void CheckIndex(int index)
        {
            EnsureAccessible();
            if (index < 0 || index >= Capacity)
            {
                throw new IndexOutOfRangeException(string.Format("index: {0} (expected: range(0, {1})", index, Capacity));
            }
        }

        protected void CheckIndex(int index, int fieldLength)
        {
            EnsureAccessible();
            if (fieldLength < 0)
            {
                throw new IndexOutOfRangeException(string.Format("length: {0} (expected: >= 0)", fieldLength));
            }

            if (index < 0 || index > Capacity - fieldLength)
            {
                throw new IndexOutOfRangeException(string.Format("index: {0}, length: {1} (expected: range(0, {2})", index, fieldLength, Capacity));
            }
        }

        protected void CheckSrcIndex(int index, int length, int srcIndex, int srcCapacity)
        {
            CheckIndex(index, length);
            if (srcIndex < 0 || srcIndex > srcCapacity - length)
            {
                throw new IndexOutOfRangeException(string.Format(
                        "srcIndex: {0}, length: {1} (expected: range(0, {2}))", srcIndex, length, srcCapacity));
            }
        }

        protected void CheckDstIndex(int index, int length, int dstIndex, int dstCapacity)
        {
            CheckIndex(index, length);
            if (dstIndex < 0 || dstIndex > dstCapacity - length)
            {
                throw new IndexOutOfRangeException(string.Format(
                        "dstIndex: {0}, length: {1} (expected: range(0, {2}))", dstIndex, length, dstCapacity));
            }
        }

        /// <summary>
        /// Throws a <see cref="IndexOutOfRangeException"/> if the current <see cref="ReadableBytes"/> of this buffer
        /// is less than <see cref="minimumReadableBytes"/>.
        /// </summary>
        protected void CheckReadableBytes(int minimumReadableBytes)
        {
            EnsureAccessible();
            if (minimumReadableBytes < 0) throw new ArgumentOutOfRangeException("minimumReadableBytes", string.Format("minimumReadableBytes: {0} (expected: >= 0)", minimumReadableBytes));

            if(ReaderIndex > WriterIndex - minimumReadableBytes)
                throw new IndexOutOfRangeException(string.Format(
                    "readerIndex({0}) + length({1}) exceeds writerIndex({2}): {3}",
                    ReaderIndex, minimumReadableBytes, WriterIndex, this));
        }

        protected void EnsureAccessible()
        {
            //TODO: add reference counting in the future if applicable
        }

        private sealed class ByteBufEqualityComparer : IEqualityComparer<IByteBuf>
        {
            public bool Equals(IByteBuf x, IByteBuf y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.ReadableBytes == 0 && y.ReadableBytes == 0) return true;
                if (x.ReadableBytes != y.ReadableBytes) return false;

                var readAllBytesX = x.ToArray();
                var readAllBytesY = y.ToArray();
                return readAllBytesX.SequenceEqual(readAllBytesY);
            }

            public int GetHashCode(IByteBuf obj)
            {
                return obj.GetHashCode();
            }
        }

        private static readonly IEqualityComparer<IByteBuf> ByteBufComparerInstance = new ByteBufEqualityComparer();

        public static IEqualityComparer<IByteBuf> ByteBufComparer => ByteBufComparerInstance;
    }
}