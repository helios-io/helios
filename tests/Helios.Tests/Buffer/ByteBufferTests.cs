using System.Linq;
using System.Text;
using Helios.Buffers;
using NUnit.Framework;

namespace Helios.Tests.Buffer
{
    [TestFixture]
    public class ByteBufferTests
    {
        protected virtual IByteBuf GetBuffer(int initialCapacity, int maxCapacity)
        {
            return ByteBuffer.AllocateDirect(initialCapacity, maxCapacity);
        }

        protected virtual IByteBuf GetBuffer(int initialCapacity)
        {
            return ByteBuffer.AllocateDirect(initialCapacity);
        }

        #region Data type tests

        [Test]
        public void Should_add_byte_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteByte(1).WriteByte(2);
            Assert.AreEqual(2, byteBuffer.WriterIndex);
            Assert.AreEqual((byte)1, byteBuffer.ReadByte());
            Assert.AreEqual((byte)2, byteBuffer.ReadByte());
            Assert.AreEqual(2, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_short_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteShort(1).WriteShort(2);
            Assert.AreEqual(4, byteBuffer.WriterIndex);
            Assert.AreEqual((short)1, byteBuffer.ReadShort());
            Assert.AreEqual((short)2, byteBuffer.ReadShort());
            Assert.AreEqual(4, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_ushort_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteShort(1).WriteShort(ushort.MaxValue);
            Assert.AreEqual(4, byteBuffer.WriterIndex);
            Assert.AreEqual((ushort)1, byteBuffer.ReadUnsignedShort());
            Assert.AreEqual(ushort.MaxValue, byteBuffer.ReadUnsignedShort());
            Assert.AreEqual(4, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_int_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteInt(int.MaxValue).WriteInt(int.MinValue);
            Assert.AreEqual(8, byteBuffer.WriterIndex);
            Assert.AreEqual(int.MaxValue, byteBuffer.ReadInt());
            Assert.AreEqual(int.MinValue, byteBuffer.ReadInt());
            Assert.AreEqual(8, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_uint_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            unchecked
            {
                byteBuffer.WriteInt((int)uint.MaxValue).WriteInt((int)uint.MinValue);
            }
            Assert.AreEqual(8, byteBuffer.WriterIndex);
            Assert.AreEqual(uint.MaxValue, byteBuffer.ReadUnsignedInt());
            Assert.AreEqual(uint.MinValue, byteBuffer.ReadUnsignedInt());
            Assert.AreEqual(8, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_long_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(16, 16);
            byteBuffer.WriteLong(long.MaxValue).WriteLong(long.MinValue);
            Assert.AreEqual(16, byteBuffer.WriterIndex);
            Assert.AreEqual(long.MaxValue, byteBuffer.ReadLong());
            Assert.AreEqual(long.MinValue, byteBuffer.ReadLong());
            Assert.AreEqual(16, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_char_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(10, 10);
            byteBuffer.WriteChar('a').WriteChar('c');
            Assert.AreEqual(4, byteBuffer.WriterIndex);
            Assert.AreEqual('a', byteBuffer.ReadChar());
            Assert.AreEqual('c', byteBuffer.ReadChar());
            Assert.AreEqual(4, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_double_to_ByteBuffer()
        {
            var byteBuffer = GetBuffer(16, 16);
            byteBuffer.WriteDouble(12.123d).WriteDouble(double.MinValue);
            Assert.AreEqual(16, byteBuffer.WriterIndex);
            Assert.AreEqual(12.123d, byteBuffer.ReadDouble());
            Assert.AreEqual(double.MinValue, byteBuffer.ReadDouble());
            Assert.AreEqual(16, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_add_byte_array_to_ByteBuffer()
        {
            var srcByteBuffer = Encoding.UTF8.GetBytes("hi there!");
            var byteBuffer = GetBuffer(1024);
            byteBuffer.WriteBytes(srcByteBuffer);
            Assert.AreEqual(srcByteBuffer.Length, byteBuffer.WriterIndex);
            var destinationBuffer = new byte[srcByteBuffer.Length];
            byteBuffer.ReadBytes(destinationBuffer);
            Assert.IsTrue(srcByteBuffer.SequenceEqual(destinationBuffer));
            Assert.AreEqual(srcByteBuffer.Length, byteBuffer.ReaderIndex);
        }

        [Test]
        public void Should_copy_one_ByteBuffer_to_Another_ByteBuffer()
        {
            var originalBuffer = GetBuffer(1024);
            var destinationBuffer = GetBuffer(1024);
            originalBuffer.WriteBoolean(true).WriteLong(1000L).WriteChar('a').WriteDouble(12.13d);
            originalBuffer.ReadBytes(destinationBuffer, originalBuffer.ReadableBytes);
            Assert.AreEqual(originalBuffer.WriterIndex, destinationBuffer.WriterIndex);
        }

        #endregion

        #region Capacity / Expanding

        [Test]
        public void Should_expand_ByteBuffer()
        {
            var originalByteBuffer = GetBuffer(10, 100);
            originalByteBuffer.WriteInt(12).AdjustCapacity(20).WriteInt(4);
            Assert.AreEqual(20, originalByteBuffer.Capacity);
            Assert.AreEqual(12, originalByteBuffer.ReadInt());
            Assert.AreEqual(4, originalByteBuffer.ReadInt());
        }

        [Test]
        public void Should_shrink_ByteBuffer()
        {
            var originalByteBuffer = GetBuffer(100, 100);
            originalByteBuffer.WriteInt(12).AdjustCapacity(50).WriteInt(4);
            Assert.AreEqual(50, originalByteBuffer.Capacity);
            Assert.AreEqual(12, originalByteBuffer.ReadInt());
            Assert.AreEqual(4, originalByteBuffer.ReadInt());
        }

        [Test]
        public void Should_clone_ByteBuffer()
        {
            var expectedString = "THIS IS A STRING";
            var originalByteBuffer =
                GetBuffer(100, 100).WriteInt(110).WriteBytes(Encoding.Unicode.GetBytes(expectedString));
            var clonedByteBuffer = originalByteBuffer.Duplicate();
            clonedByteBuffer.WriteBoolean(true).WriteDouble(-1113.4d);
            Assert.AreEqual(110, originalByteBuffer.ReadInt());
            Assert.AreEqual(110, clonedByteBuffer.ReadInt());
            Assert.AreEqual(expectedString, Encoding.Unicode.GetString(originalByteBuffer.ReadBytes(expectedString.Length * 2).ToArray()));
            var stringBuf = new byte[expectedString.Length*2];
            clonedByteBuffer.ReadBytes(stringBuf);
            Assert.AreEqual(expectedString, Encoding.Unicode.GetString(stringBuf));
            Assert.AreEqual(true, clonedByteBuffer.ReadBoolean());
            Assert.AreEqual(-1113.4d, clonedByteBuffer.ReadDouble());
        }

        [Test]
        public void Should_export_readable_bytes_ToArray()
        {
            var expectedString = "THIS IS A STRING";
            var originalByteBuffer =
                GetBuffer(100, 100).WriteInt(110).WriteBytes(Encoding.Unicode.GetBytes(expectedString));
            Assert.AreEqual(110, originalByteBuffer.ReadInt());
            var byteArray = originalByteBuffer.ToArray();
            Assert.AreEqual(expectedString, Encoding.Unicode.GetString(byteArray));
        }

        #endregion
    }
}
