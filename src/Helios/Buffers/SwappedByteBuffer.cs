using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Buffers
{
    public sealed class SwappedByteBuffer : IByteBuf
    {
        private readonly IByteBuf buf;
        private readonly ByteOrder order;

        public SwappedByteBuffer(IByteBuf buf)
        {
            this.buf = buf;
            if (this.buf.Endianness == ByteOrder.BigEndian)
            {
                order = ByteOrder.LittleEndian;
            }
            else
            {
                order = ByteOrder.BigEndian;
            }
        }

        public int Capacity
        {
            get { return this.buf.Capacity; }
        }

        public IByteBuf AdjustCapacity(int newCapacity)
        {
            return this.buf.AdjustCapacity(newCapacity);
        }

        public int MaxCapacity
        {
            get { return this.buf.MaxCapacity; }
        }

        public IByteBufAllocator Allocator
        {
            get { return this.buf.Allocator; }
        }

        public int ReaderIndex
        {
            get { return this.buf.ReaderIndex; }
        }

        public int WriterIndex
        {
            get { return this.buf.WriterIndex; }
        }

        public IByteBuf SetWriterIndex(int writerIndex)
        {
            this.buf.SetWriterIndex(writerIndex);
            return this;
        }

        public IByteBuf SetReaderIndex(int readerIndex)
        {
            this.buf.SetReaderIndex(readerIndex);
            return this;
        }

        public IByteBuf SetIndex(int readerIndex, int writerIndex)
        {
            this.buf.SetIndex(readerIndex, writerIndex);
            return this;
        }

        public int ReadableBytes
        {
            get { return this.buf.ReadableBytes; }
        }

        public int WritableBytes
        {
            get { return this.buf.WritableBytes; }
        }

        public int MaxWritableBytes
        {
            get { return this.buf.MaxWritableBytes; }
        }

        public bool IsReadable()
        {
            return this.buf.IsReadable();
        }

        public bool IsReadable(int size)
        {
            return this.buf.IsReadable(size);
        }

        public bool IsWritable()
        {
            return this.buf.IsWritable();
        }

        public bool IsWritable(int size)
        {
            return this.buf.IsWritable(size);
        }

        public IByteBuf Clear()
        {
            this.buf.Clear();
            return this;
        }

        public IByteBuf MarkReaderIndex()
        {
            this.buf.MarkReaderIndex();
            return this;
        }

        public IByteBuf ResetReaderIndex()
        {
            this.buf.ResetReaderIndex();
            return this;
        }

        public IByteBuf MarkWriterIndex()
        {
            this.buf.MarkWriterIndex();
            return this;
        }

        public IByteBuf ResetWriterIndex()
        {
            this.buf.ResetWriterIndex();
            return this;
        }

        public IByteBuf DiscardReadBytes()
        {
            this.buf.DiscardReadBytes();
            return this;
        }

        public IByteBuf DiscardSomeReadBytes()
        {
            throw new NotImplementedException();
        }

        public IByteBuf EnsureWritable(int minWritableBytes)
        {
            this.buf.EnsureWritable(minWritableBytes);
            return this;
        }

        public int EnsureWritable(int minWritableBytes, bool force)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int index)
        {
            return this.buf.GetBoolean(index);
        }

        public byte GetByte(int index)
        {
            return this.buf.GetByte(index);
        }

        public short GetShort(int index)
        {
            return ByteBufferUtil.SwapShort(this.buf.GetShort(index));
        }

        public ushort GetUnsignedShort(int index)
        {
            unchecked
            {
                return (ushort)(this.GetShort(index));
            }
        }

        public int GetInt(int index)
        {
            return ByteBufferUtil.SwapInt(this.buf.GetInt(index));
        }

        public uint GetUnsignedInt(int index)
        {
            unchecked
            {
                return (uint)this.GetInt(index);
            }
        }

        public long GetLong(int index)
        {
            return ByteBufferUtil.SwapLong(this.buf.GetLong(index));
        }

        public char GetChar(int index)
        {
            return (char)this.GetShort(index);
        }

        public double GetDouble(int index)
        {
            return BitConverter.Int64BitsToDouble(this.GetLong(index));
        }

        public IByteBuf GetBytes(int index, IByteBuf destination)
        {
            this.buf.GetBytes(index, destination);
            return this;
        }

        public IByteBuf GetBytes(int index, IByteBuf destination, int length)
        {
            this.buf.GetBytes(index, destination, length);
            return this;
        }

        public IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            this.buf.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public IByteBuf GetBytes(int index, byte[] destination)
        {
            this.buf.GetBytes(index, destination);
            return this;
        }

        public IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            this.buf.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public IByteBuf SetBoolean(int index, bool value)
        {
            this.buf.SetBoolean(index, value);
            return this;
        }

        public IByteBuf SetByte(int index, int value)
        {
            this.buf.SetByte(index, value);
            return this;
        }

        public IByteBuf SetShort(int index, int value)
        {
            this.buf.SetShort(index, ByteBufferUtil.SwapShort((short)value));
            return this;
        }

        public IByteBuf SetUnsignedShort(int index, ushort value)
        {
            throw new NotImplementedException();
        }

        public IByteBuf SetUnsignedShort(int index, int value)
        {
            unchecked
            {
                this.buf.SetUnsignedShort(index, (ushort)ByteBufferUtil.SwapShort((short)value));
            }
            return this;
        }

        public IByteBuf SetInt(int index, int value)
        {
            this.buf.SetInt(index, ByteBufferUtil.SwapInt(value));
            return this;
        }

        public IByteBuf SetUnsignedInt(int index, uint value)
        {
            unchecked
            {
                this.buf.SetUnsignedInt(index, (uint)ByteBufferUtil.SwapInt((int)value));
            }
            return this;
        }

        public IByteBuf SetLong(int index, long value)
        {
            this.buf.SetLong(index, ByteBufferUtil.SwapLong(value));
            return this;
        }

        public IByteBuf SetChar(int index, char value)
        {
            this.SetShort(index, (short)value);
            return this;
        }

        public IByteBuf SetDouble(int index, double value)
        {
            this.SetLong(index, BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public IByteBuf SetBytes(int index, IByteBuf src)
        {
            this.buf.SetBytes(index, src);
            return this;
        }

        public IByteBuf SetBytes(int index, IByteBuf src, int length)
        {
            this.buf.SetBytes(index, src, length);
            return this;
        }

        public IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            this.buf.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public IByteBuf SetBytes(int index, byte[] src)
        {
            this.buf.SetBytes(index, src);
            return this;
        }

        public IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            this.buf.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public bool ReadBoolean()
        {
            return this.buf.ReadBoolean();
        }

        public byte ReadByte()
        {
            return this.buf.ReadByte();
        }

        public short ReadShort()
        {
            return ByteBufferUtil.SwapShort(this.buf.ReadShort());
        }

        public ushort ReadUnsignedShort()
        {
            unchecked
            {
                return (ushort)this.ReadShort();
            }
        }

        public int ReadInt()
        {
            return ByteBufferUtil.SwapInt(this.buf.ReadInt());
        }

        public uint ReadUnsignedInt()
        {
            unchecked
            {
                return (uint)this.ReadInt();
            }
        }

        public long ReadLong()
        {
            return ByteBufferUtil.SwapLong(this.buf.ReadLong());
        }

        public char ReadChar()
        {
            return (char)this.ReadShort();
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(this.ReadLong());
        }

        public IByteBuf ReadBytes(int length)
        {
            return this.buf.ReadBytes(length).WithOrder(this.Order);
        }

        public IByteBuf ReadBytes(IByteBuf destination)
        {
            this.buf.ReadBytes(destination);
            return this;
        }

        public IByteBuf ReadBytes(IByteBuf destination, int length)
        {
            this.buf.ReadBytes(destination, length);
            return this;
        }

        public IByteBuf ReadBytes(IByteBuf destination, int dstIndex, int length)
        {
            this.buf.ReadBytes(destination, dstIndex, length);
            return this;
        }

        public IByteBuf ReadBytes(byte[] destination)
        {
            this.buf.ReadBytes(destination);
            return this;
        }

        public IByteBuf ReadBytes(byte[] destination, int dstIndex, int length)
        {
            this.buf.ReadBytes(destination, dstIndex, length);
            return this;
        }

        public IByteBuf SkipBytes(int length)
        {
            this.buf.SkipBytes(length);
            return this;
        }

        public IByteBuf WriteBoolean(bool value)
        {
            this.buf.WriteBoolean(value);
            return this;
        }

        public IByteBuf WriteByte(int value)
        {
            this.buf.WriteByte(value);
            return this;
        }

        public IByteBuf WriteShort(int value)
        {
            this.buf.WriteShort(ByteBufferUtil.SwapShort((short)value));
            return this;
        }

        public IByteBuf WriteUnsignedShort(ushort value)
        {
            throw new NotImplementedException();
        }

        public IByteBuf WriteUnsignedShort(int value)
        {
            this.buf.WriteUnsignedShort(unchecked((ushort)ByteBufferUtil.SwapShort((short)value)));
            return this;
        }

        public IByteBuf WriteInt(int value)
        {
            this.buf.WriteInt(ByteBufferUtil.SwapInt(value));
            return this;
        }

        public IByteBuf WriteUnsignedInt(uint value)
        {
            unchecked
            {
                this.buf.WriteUnsignedInt((uint)ByteBufferUtil.SwapInt((int)value));
            }

            return this;
        }

        public IByteBuf WriteLong(long value)
        {
            this.buf.WriteLong(ByteBufferUtil.SwapLong(value));
            return this;
        }

        public IByteBuf WriteChar(char value)
        {
            this.WriteShort(value);
            return this;
        }

        public IByteBuf WriteDouble(double value)
        {
            this.WriteLong(BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public IByteBuf WriteBytes(IByteBuf src)
        {
            this.buf.WriteBytes(src);
            return this;
        }

        public IByteBuf WriteBytes(IByteBuf src, int length)
        {
            this.buf.WriteBytes(src, length);
            return this;
        }

        public IByteBuf WriteBytes(IByteBuf src, int srcIndex, int length)
        {
            this.buf.WriteBytes(src, srcIndex, length);
            return this;
        }

        public IByteBuf WriteBytes(byte[] src)
        {
            this.buf.WriteBytes(src);
            return this;
        }

        public IByteBuf WriteBytes(byte[] src, int srcIndex, int length)
        {
            this.buf.WriteBytes(src, srcIndex, length);
            return this;
        }

        public bool HasArray
        {
            get { return this.buf.HasArray; }
        }

        public byte[] ToArray()
        {
            return this.buf.ToArray().Reverse().ToArray();
        }

        public IByteBuf Duplicate()
        {
            return this.buf.Duplicate().WithOrder(this.Order);
        }

        public IByteBuf Unwrap()
        {
            return this.buf.Unwrap();
        }

        public ByteOrder Order
        {
            get { return this.order; }
        }

        public IByteBuf WithOrder(ByteOrder endianness)
        {
            if (endianness == this.Order)
            {
                return this;
            }
            return this.buf;
        }

        public IByteBuf Copy()
        {
            return this.buf.Copy().WithOrder(this.Order);
        }

        public IByteBuf Copy(int index, int length)
        {
            return this.buf.Copy(index, length).WithOrder(this.Order);
        }
        public override string ToString()
        {
            return "Swapped(" + this.buf + ")";
        }
    }
}
