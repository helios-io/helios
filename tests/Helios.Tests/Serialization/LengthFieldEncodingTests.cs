using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Helios.Buffers;
using Helios.Exceptions;
using Helios.Net;
using Helios.Serialization;
using Helios.Topology;
using NUnit.Framework;

namespace Helios.Tests.Serialization
{
    [TestFixture]
    public class LengthFieldEncodingTests
    {
        #region Setup / Teardown

        protected int LengthFieldLength = 4;
        protected IMessageEncoder Encoder;
        protected IMessageDecoder Decoder;
        protected IConnection TestConnection = new DummyConnection(UnpooledByteBufAllocator.Default);

        [TestFixtureSetUp]
        public void Setup()
        {
            Encoder = new LengthFieldPrepender(LengthFieldLength);
            Decoder = new LengthFieldFrameBasedDecoder(2000,0,LengthFieldLength,0,LengthFieldLength);  //stip headers
        }

        #endregion

        #region Tests

        [Test]
        public void Should_encode_length_in_message()
        {
            var binaryContent = Encoding.UTF8.GetBytes("somebytes");
            var expectedBytes = binaryContent.Length;
            var data = ByteBuffer.AllocateDirect(expectedBytes).WriteBytes(binaryContent);

            List<IByteBuf> encodedMessages;
            Encoder.Encode(TestConnection, data, out encodedMessages);

            var encodedData = encodedMessages[0];
            Assert.AreEqual(expectedBytes + 4, encodedData.ReadableBytes);
            var decodedLength = encodedData.ReadInt();
            Assert.AreEqual(expectedBytes, decodedLength);
        }

        [Test]
        public void Should_decode_single_lengthFramed_message()
        {
            var binaryContent = Encoding.UTF8.GetBytes("somebytes");
            var expectedBytes = binaryContent.Length;

            var data = ByteBuffer.AllocateDirect(expectedBytes).WriteBytes(binaryContent);

            List<IByteBuf> encodedMessages;
            Encoder.Encode(TestConnection, data, out encodedMessages);

            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, encodedMessages[0], out decodedMessages);

            Assert.IsTrue(binaryContent.SequenceEqual(decodedMessages[0].ToArray()));
        }

        [Test]
        public void Should_decoded_multiple_lengthFramed_messages()
        {
            var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
            var binaryContent2 = Encoding.UTF8.GetBytes("moarbytes");
            var binaryContent3 = BitConverter.GetBytes(100034034L);

            var buffer = ByteBuffer.AllocateDirect(100).WriteInt(binaryContent1.Length)
                .WriteBytes(binaryContent1).WriteInt(binaryContent2.Length).WriteBytes(binaryContent2)
                .WriteInt(binaryContent3.Length).WriteBytes(binaryContent3);


            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, buffer, out decodedMessages);
            Assert.AreEqual(3, decodedMessages.Count);
            Assert.IsTrue(binaryContent1.SequenceEqual(decodedMessages[0].ToArray()));
            Assert.IsTrue(binaryContent2.SequenceEqual(decodedMessages[1].ToArray()));
            Assert.IsTrue(binaryContent3.SequenceEqual(decodedMessages[2].ToArray()));
        }

        [Test]
        [ExpectedException(typeof(CorruptedFrameException))]
        public void Should_throw_exception_when_decoding_negative_frameLength()
        {
            var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
            var buffer = ByteBuffer.AllocateDirect(100)
                .WriteInt((-1)*binaryContent1.Length) //make the frame length negative
                .WriteBytes(binaryContent1);

            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, buffer, out decodedMessages);
            
        }

        [Test]
        public void Should_throw_exception_when_decoding_zero_frameLength()
        {
            var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
            var buffer = ByteBuffer.AllocateDirect(100)
                .WriteInt(0) //make the frame length negative
                .WriteBytes(binaryContent1);

            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, buffer, out decodedMessages);
            Assert.Pass("Helios should be in byte disacarding mode now");
        }

        #endregion
    }
}
