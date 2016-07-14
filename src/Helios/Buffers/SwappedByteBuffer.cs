// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Linq;
using System.Text;

namespace Helios.Buffers
{
    public sealed class SwappedByteBuffer : IByteBuf
    {
        private readonly IByteBuf _buf;

        public SwappedByteBuffer(IByteBuf buf)
        {
            _buf = buf;
            if (buf.Order == ByteOrder.BigEndian)
            {
                Order = ByteOrder.LittleEndian;
            }
            else
            {
                Order = ByteOrder.BigEndian;
            }
        }

        public int Capacity
        {
            get { return _buf.Capacity; }
        }

        public IByteBuf AdjustCapacity(int newCapacity)
        {
            return _buf.AdjustCapacity(newCapacity);
        }

        public int MaxCapacity
        {
            get { return _buf.MaxCapacity; }
        }

        public IByteBufAllocator Allocator
        {
            get { return _buf.Allocator; }
        }

        public int ReaderIndex
        {
            get { return _buf.ReaderIndex; }
        }

        public int WriterIndex
        {
            get { return _buf.WriterIndex; }
        }

        public IByteBuf SetWriterIndex(int writerIndex)
        {
            _buf.SetWriterIndex(writerIndex);
            return this;
        }

        public IByteBuf SetReaderIndex(int readerIndex)
        {
            _buf.SetReaderIndex(readerIndex);
            return this;
        }

        public IByteBuf SetIndex(int readerIndex, int writerIndex)
        {
            _buf.SetIndex(readerIndex, writerIndex);
            return this;
        }

        public int IoBufferCount => this._buf.IoBufferCount;

        public ArraySegment<byte> GetIoBuffer() => this._buf.GetIoBuffer();

        public ArraySegment<byte> GetIoBuffer(int index, int length) => this._buf.GetIoBuffer(index, length);

        public ArraySegment<byte>[] GetIoBuffers() => this._buf.GetIoBuffers();

        public ArraySegment<byte>[] GetIoBuffers(int index, int length) => this._buf.GetIoBuffers(index, length);

        public int ReadableBytes
        {
            get { return _buf.ReadableBytes; }
        }

        public int WritableBytes
        {
            get { return _buf.WritableBytes; }
        }

        public int MaxWritableBytes
        {
            get { return _buf.MaxWritableBytes; }
        }

        public bool IsReadable()
        {
            return _buf.IsReadable();
        }

        public bool IsReadable(int size)
        {
            return _buf.IsReadable(size);
        }

        public bool IsWritable()
        {
            return _buf.IsWritable();
        }

        public bool IsWritable(int size)
        {
            return _buf.IsWritable(size);
        }

        public IByteBuf Clear()
        {
            _buf.Clear();
            return this;
        }

        public IByteBuf MarkReaderIndex()
        {
            _buf.MarkReaderIndex();
            return this;
        }

        public IByteBuf ResetReaderIndex()
        {
            _buf.ResetReaderIndex();
            return this;
        }

        public IByteBuf MarkWriterIndex()
        {
            _buf.MarkWriterIndex();
            return this;
        }

        public IByteBuf ResetWriterIndex()
        {
            _buf.ResetWriterIndex();
            return this;
        }

        public IByteBuf DiscardReadBytes()
        {
            _buf.DiscardReadBytes();
            return this;
        }

        public IByteBuf DiscardSomeReadBytes()
        {
            _buf.DiscardSomeReadBytes();
            return this;
        }

        public IByteBuf EnsureWritable(int minWritableBytes)
        {
            _buf.EnsureWritable(minWritableBytes);
            return this;
        }

        public bool GetBoolean(int index)
        {
            return _buf.GetBoolean(index);
        }

        public byte GetByte(int index)
        {
            return _buf.GetByte(index);
        }

        public short GetShort(int index)
        {
            return ByteBufferUtil.SwapShort(_buf.GetShort(index));
        }

        public ushort GetUnsignedShort(int index)
        {
            unchecked
            {
                return (ushort) GetShort(index);
            }
        }

        public int GetInt(int index)
        {
            return ByteBufferUtil.SwapInt(_buf.GetInt(index));
        }

        public uint GetUnsignedInt(int index)
        {
            unchecked
            {
                return (uint) GetInt(index);
            }
        }

        public long GetLong(int index)
        {
            return ByteBufferUtil.SwapLong(_buf.GetLong(index));
        }

        public char GetChar(int index)
        {
            return (char) GetShort(index);
        }

        public double GetDouble(int index)
        {
            return BitConverter.Int64BitsToDouble(GetLong(index));
        }

        public IByteBuf GetBytes(int index, IByteBuf destination)
        {
            _buf.GetBytes(index, destination);
            return this;
        }

        public IByteBuf GetBytes(int index, IByteBuf destination, int length)
        {
            _buf.GetBytes(index, destination, length);
            return this;
        }

        public IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            _buf.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public IByteBuf GetBytes(int index, byte[] destination)
        {
            _buf.GetBytes(index, destination);
            return this;
        }

        public IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            _buf.GetBytes(index, destination, dstIndex, length);
            return this;
        }

        public IByteBuf SetBoolean(int index, bool value)
        {
            _buf.SetBoolean(index, value);
            return this;
        }

        public IByteBuf SetByte(int index, int value)
        {
            _buf.SetByte(index, value);
            return this;
        }

        public IByteBuf SetShort(int index, int value)
        {
            _buf.SetShort(index, ByteBufferUtil.SwapShort((short) value));
            return this;
        }

        public IByteBuf SetUnsignedShort(int index, int value)
        {
            unchecked
            {
                _buf.SetUnsignedShort(index, (ushort) ByteBufferUtil.SwapShort((short) value));
            }
            return this;
        }

        public IByteBuf SetInt(int index, int value)
        {
            _buf.SetInt(index, ByteBufferUtil.SwapInt(value));
            return this;
        }

        public IByteBuf SetUnsignedInt(int index, uint value)
        {
            unchecked
            {
                _buf.SetUnsignedInt(index, (uint) ByteBufferUtil.SwapInt((int) value));
            }
            return this;
        }

        public IByteBuf SetLong(int index, long value)
        {
            _buf.SetLong(index, ByteBufferUtil.SwapLong(value));
            return this;
        }

        public IByteBuf SetChar(int index, char value)
        {
            SetShort(index, (short) value);
            return this;
        }

        public IByteBuf SetDouble(int index, double value)
        {
            SetLong(index, BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public IByteBuf SetBytes(int index, IByteBuf src)
        {
            _buf.SetBytes(index, src);
            return this;
        }

        public IByteBuf SetBytes(int index, IByteBuf src, int length)
        {
            _buf.SetBytes(index, src, length);
            return this;
        }

        public IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            _buf.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public IByteBuf SetBytes(int index, byte[] src)
        {
            _buf.SetBytes(index, src);
            return this;
        }

        public IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            _buf.SetBytes(index, src, srcIndex, length);
            return this;
        }

        public bool ReadBoolean()
        {
            return _buf.ReadBoolean();
        }

        public byte ReadByte()
        {
            return _buf.ReadByte();
        }

        public short ReadShort()
        {
            return ByteBufferUtil.SwapShort(_buf.ReadShort());
        }

        public ushort ReadUnsignedShort()
        {
            unchecked
            {
                return (ushort) ReadShort();
            }
        }

        public int ReadInt()
        {
            return ByteBufferUtil.SwapInt(_buf.ReadInt());
        }

        public uint ReadUnsignedInt()
        {
            unchecked
            {
                return (uint) ReadInt();
            }
        }

        public long ReadLong()
        {
            return ByteBufferUtil.SwapLong(_buf.ReadLong());
        }

        public char ReadChar()
        {
            return (char) ReadShort();
        }

        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        public IByteBuf ReadBytes(int length)
        {
            return _buf.ReadBytes(length).WithOrder(Order);
        }

        public IByteBuf ReadBytes(IByteBuf destination)
        {
            _buf.ReadBytes(destination);
            return this;
        }

        public IByteBuf ReadBytes(IByteBuf destination, int length)
        {
            _buf.ReadBytes(destination, length);
            return this;
        }

        public IByteBuf ReadBytes(IByteBuf destination, int dstIndex, int length)
        {
            _buf.ReadBytes(destination, dstIndex, length);
            return this;
        }

        public IByteBuf ReadBytes(byte[] destination)
        {
            _buf.ReadBytes(destination);
            return this;
        }

        public IByteBuf ReadBytes(byte[] destination, int dstIndex, int length)
        {
            _buf.ReadBytes(destination, dstIndex, length);
            return this;
        }

        public IByteBuf SkipBytes(int length)
        {
            _buf.SkipBytes(length);
            return this;
        }

        public IByteBuf WriteBoolean(bool value)
        {
            _buf.WriteBoolean(value);
            return this;
        }

        public IByteBuf WriteByte(int value)
        {
            _buf.WriteByte(value);
            return this;
        }

        public IByteBuf WriteShort(int value)
        {
            _buf.WriteShort(ByteBufferUtil.SwapShort((short) value));
            return this;
        }

        public IByteBuf WriteUnsignedShort(int value)
        {
            _buf.WriteUnsignedShort(unchecked((ushort) ByteBufferUtil.SwapShort((short) value)));
            return this;
        }

        public IByteBuf WriteInt(int value)
        {
            _buf.WriteInt(ByteBufferUtil.SwapInt(value));
            return this;
        }

        public IByteBuf WriteUnsignedInt(uint value)
        {
            unchecked
            {
                _buf.WriteUnsignedInt((uint) ByteBufferUtil.SwapInt((int) value));
            }

            return this;
        }

        public IByteBuf WriteLong(long value)
        {
            _buf.WriteLong(ByteBufferUtil.SwapLong(value));
            return this;
        }

        public IByteBuf WriteChar(char value)
        {
            WriteShort(value);
            return this;
        }

        public IByteBuf WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
            return this;
        }

        public IByteBuf WriteBytes(IByteBuf src)
        {
            _buf.WriteBytes(src);
            return this;
        }

        public IByteBuf WriteBytes(IByteBuf src, int length)
        {
            _buf.WriteBytes(src, length);
            return this;
        }

        public IByteBuf WriteBytes(IByteBuf src, int srcIndex, int length)
        {
            _buf.WriteBytes(src, srcIndex, length);
            return this;
        }

        public IByteBuf WriteBytes(byte[] src)
        {
            _buf.WriteBytes(src);
            return this;
        }

        public IByteBuf WriteBytes(byte[] src, int srcIndex, int length)
        {
            _buf.WriteBytes(src, srcIndex, length);
            return this;
        }

        public IByteBuf WriteZero(int length)
        {
            _buf.WriteZero(length);
            return this;
        }

        public bool HasArray
        {
            get { return _buf.HasArray; }
        }

        public byte[] Array
        {
            get { return _buf.Array; }
        }

        public byte[] ToArray()
        {
            return _buf.ToArray().Reverse().ToArray();
        }

        public bool IsDirect
        {
            get { return _buf.IsDirect; }
        }

        public IByteBuf ReadSlice(int length)
        {
            return _buf.ReadSlice(length).WithOrder(Order);
        }

        public IByteBuf Duplicate()
        {
            return _buf.Duplicate().WithOrder(Order);
        }

        public IByteBuf Unwrap()
        {
            return _buf.Unwrap();
        }

        public IByteBuf Compact()
        {
            throw new NotImplementedException();
        }

        public IByteBuf CompactIfNecessary()
        {
            throw new NotImplementedException();
        }

        public string ToString(Encoding encoding)
        {
            return ByteBufferUtil.DecodeString(this, ReaderIndex, ReadableBytes, encoding);
        }

        public ByteOrder Order { get; }

        public IByteBuf WithOrder(ByteOrder endianness)
        {
            if (endianness == Order)
            {
                return this;
            }
            return _buf;
        }

        public IByteBuf Copy()
        {
            return _buf.Copy().WithOrder(Order);
        }

        public IByteBuf Copy(int index, int length)
        {
            return _buf.Copy(index, length).WithOrder(Order);
        }

        public IByteBuf Slice()
        {
            return _buf.Slice().WithOrder(Order);
        }

        public IByteBuf Slice(int index, int length)
        {
            return _buf.Slice(index, length).WithOrder(Order);
        }

        public int ArrayOffset { get; }

        public int ReferenceCount
        {
            get { return _buf.ReferenceCount; }
        }

        public IReferenceCounted Retain()
        {
            return _buf.Retain();
        }

        public IReferenceCounted Retain(int increment)
        {
            return _buf.Retain(increment);
        }

        public IReferenceCounted Touch()
        {
            return _buf.Touch();
        }

        public IReferenceCounted Touch(object hint)
        {
            return _buf.Touch(hint);
        }

        public bool Release()
        {
            return _buf.Release();
        }

        public bool Release(int decrement)
        {
            return _buf.Release(decrement);
        }

        public IByteBuf SetUnsignedShort(int index, ushort value)
        {
            return SetUnsignedShort(index, (int) value);
        }

        public IByteBuf WriteUnsignedShort(ushort value)
        {
            return WriteUnsignedShort((int) value);
        }

        public override string ToString()
        {
            return "Swapped(" + _buf + ")";
        }

        public override int GetHashCode()
        {
            return _buf.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _buf.Equals(obj);
        }
    }
}