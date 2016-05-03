﻿using System.Linq;
using System.Text;
using Helios.Buffers;
using Xunit;

namespace Helios.Tests.Buffer
{
    public abstract class ByteBufferTests
    {
        protected abstract IByteBuf GetBuffer(int initialCapacity, int maxCapacity);

        protected abstract IByteBuf GetBuffer(int initialCapacity);

        #region Data type tests

        [Fact]
        public void Should_add_byte_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteByte(1).WriteByte(2);
            Assert.Equal(2, byteBuffer.WriterIndex);
            Assert.Equal((byte) 1, byteBuffer.ReadByte());
            Assert.Equal((byte) 2, byteBuffer.ReadByte());
            Assert.Equal(2, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_short_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteShort(1).WriteShort(2);
            Assert.Equal(4, byteBuffer.WriterIndex);
            Assert.Equal((short) 1, byteBuffer.ReadShort());
            Assert.Equal((short) 2, byteBuffer.ReadShort());
            Assert.Equal(4, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_ushort_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteShort(1).WriteShort(ushort.MaxValue);
            Assert.Equal(4, byteBuffer.WriterIndex);
            Assert.Equal((ushort) 1, byteBuffer.ReadUnsignedShort());
            Assert.Equal(ushort.MaxValue, byteBuffer.ReadUnsignedShort());
            Assert.Equal(4, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_int_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteInt(int.MaxValue).WriteInt(int.MinValue);
            Assert.Equal(8, byteBuffer.WriterIndex);
            Assert.Equal(int.MaxValue, byteBuffer.ReadInt());
            Assert.Equal(int.MinValue, byteBuffer.ReadInt());
            Assert.Equal(8, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_uint_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            unchecked
            {
                byteBuffer.WriteInt((int) uint.MaxValue).WriteInt((int) uint.MinValue);
            }
            Assert.Equal(8, byteBuffer.WriterIndex);
            Assert.Equal(uint.MaxValue, byteBuffer.ReadUnsignedInt());
            Assert.Equal(uint.MinValue, byteBuffer.ReadUnsignedInt());
            Assert.Equal(8, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_long_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(16, 16);
            byteBuffer.WriteLong(long.MaxValue).WriteLong(long.MinValue);
            Assert.Equal(16, byteBuffer.WriterIndex);
            Assert.Equal(long.MaxValue, byteBuffer.ReadLong());
            Assert.Equal(long.MinValue, byteBuffer.ReadLong());
            Assert.Equal(16, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_char_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteChar('a').WriteChar('c');
            Assert.Equal(4, byteBuffer.WriterIndex);
            Assert.Equal('a', byteBuffer.ReadChar());
            Assert.Equal('c', byteBuffer.ReadChar());
            Assert.Equal(4, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_double_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(16, 16);
            byteBuffer.WriteDouble(12.123d).WriteDouble(double.MinValue);
            Assert.Equal(16, byteBuffer.WriterIndex);
            Assert.Equal(12.123d, byteBuffer.ReadDouble());
            Assert.Equal(double.MinValue, byteBuffer.ReadDouble());
            Assert.Equal(16, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_add_byte_array_to_ByteBuffer()
        {
            var srcByteBuffer = Encoding.UTF8.GetBytes("hi there!");
            var byteBuffer = GetBuffer(1024);
            byteBuffer.WriteBytes(srcByteBuffer);
            Assert.Equal(srcByteBuffer.Length, byteBuffer.WriterIndex);
            var destinationBuffer = new byte[srcByteBuffer.Length];
            byteBuffer.ReadBytes(destinationBuffer);
            Assert.True(srcByteBuffer.SequenceEqual(destinationBuffer));
            Assert.Equal(srcByteBuffer.Length, byteBuffer.ReaderIndex);
        }

        [Fact]
        public void Should_copy_one_ByteBuffer_to_Another_ByteBuffer()
        {
            var originalBuffer = GetBuffer(1024);
            var destinationBuffer = GetBuffer(1024);
            originalBuffer.WriteBoolean(true).WriteLong(1000L).WriteChar('a').WriteDouble(12.13d);
            originalBuffer.ReadBytes(destinationBuffer, originalBuffer.ReadableBytes);
            Assert.Equal(originalBuffer.WriterIndex, destinationBuffer.WriterIndex);
        }

        #endregion

        #region Capacity / Expanding

        [Fact]
        public void Should_expand_ByteBuffer()
        {
            var originalByteBuffer = GetBuffer(10, 100);
            originalByteBuffer.WriteInt(12).AdjustCapacity(20).WriteInt(4);
            Assert.Equal(20, originalByteBuffer.Capacity);
            Assert.Equal(12, originalByteBuffer.ReadInt());
            Assert.Equal(4, originalByteBuffer.ReadInt());
        }

        [Fact]
        public void Should_shrink_ByteBuffer()
        {
            var originalByteBuffer = GetBuffer(100, 100);
            originalByteBuffer.WriteInt(12).AdjustCapacity(50).WriteInt(4);
            Assert.Equal(50, originalByteBuffer.Capacity);
            Assert.Equal(12, originalByteBuffer.ReadInt());
            Assert.Equal(4, originalByteBuffer.ReadInt());
        }

        [Fact]
        public void Should_clone_ByteBuffer()
        {
            var expectedString = "THIS IS A STRING";
            var originalByteBuffer =
                GetBuffer(100, 100).WriteInt(110).WriteBytes(Encoding.Unicode.GetBytes(expectedString));
            var clonedByteBuffer = originalByteBuffer.Duplicate();
            clonedByteBuffer.WriteBoolean(true).WriteDouble(-1113.4d);
            Assert.Equal(110, originalByteBuffer.ReadInt());
            Assert.Equal(110, clonedByteBuffer.ReadInt());
            Assert.Equal(expectedString,
                Encoding.Unicode.GetString(originalByteBuffer.ReadBytes(expectedString.Length*2).ToArray()));
            var stringBuf = new byte[expectedString.Length*2];
            clonedByteBuffer.ReadBytes(stringBuf);
            Assert.Equal(expectedString, Encoding.Unicode.GetString(stringBuf));
            Assert.Equal(true, clonedByteBuffer.ReadBoolean());
            Assert.Equal(-1113.4d, clonedByteBuffer.ReadDouble());
        }

        [Fact]
        public void Should_export_readable_bytes_ToArray()
        {
            var expectedString = "THIS IS A STRING";
            var originalByteBuffer =
                GetBuffer(100, 100).WriteInt(110).WriteBytes(Encoding.Unicode.GetBytes(expectedString));
            Assert.Equal(110, originalByteBuffer.ReadInt());
            var byteArray = originalByteBuffer.ToArray();
            Assert.Equal(expectedString, Encoding.Unicode.GetString(byteArray));
        }

        #endregion
    }
}
