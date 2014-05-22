using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        [TestFixtureSetUp]
        public void Setup()
        {
            Encoder = new LengthFieldPrepender(LengthFieldLength);
            Decoder = new LengthFieldFrameBasedDecoder(2000,0,LengthFieldLength);
        }

        #endregion

        #region Tests

        [Test]
        public void Should_encode_length_in_message()
        {
            var binaryContent = Encoding.UTF8.GetBytes("somebytes");
            var expectedBytes = binaryContent.Length;
            var networkData = new NetworkData()
            {
                Buffer = binaryContent,
                Length = expectedBytes,
                RemoteHost = NodeBuilder.BuildNode().Host("host1.com").WithPort(1000)
            };

            List<NetworkData> encodedMessages;
            Encoder.Encode(networkData, out encodedMessages);

            var encodedData = encodedMessages[0];
            Assert.AreEqual(networkData.Length + 4, encodedData.Length);
            var decodedLength = BitConverter.ToInt32(encodedData.Buffer, 0);
            Assert.AreEqual(networkData.Length, decodedLength);
        }

        [Test]
        public void Should_decode_single_lengthFramed_message()
        {
            var binaryContent = Encoding.UTF8.GetBytes("somebytes");
            var expectedBytes = binaryContent.Length;
            var networkData = new NetworkData()
            {
                Buffer = binaryContent,
                Length = expectedBytes,
                RemoteHost = NodeBuilder.BuildNode().Host("host1.com").WithPort(1000)
            };

            List<NetworkData> encodedMessages;
            Encoder.Encode(networkData, out encodedMessages);

            List<NetworkData> decodedMessages;
            Decoder.Decode(encodedMessages[0], out decodedMessages);

            Assert.IsTrue(binaryContent.SequenceEqual(decodedMessages[0].Buffer));
        }

        [Test]
        public void Should_decoded_multiple_lengthFramed_messages()
        {
            var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
            var binaryContent2 = Encoding.UTF8.GetBytes("moarbytes");
            var binaryContent3 = BitConverter.GetBytes(100034034L);

            var multiMessageBuffer = new byte[0];
            var length = 0;
            using (var memoryStream = new MemoryStream())
            {

                memoryStream.Write(BitConverter.GetBytes((uint)binaryContent1.Length), 0, 4);
                memoryStream.Write(binaryContent1, 0, binaryContent1.Length);
                length = 4 + binaryContent1.Length;

                memoryStream.Write(BitConverter.GetBytes((uint)binaryContent2.Length), 0, 4);
                memoryStream.Write(binaryContent2, 0, binaryContent2.Length);
                length += 4 + binaryContent2.Length;

                memoryStream.Write(BitConverter.GetBytes((uint)binaryContent3.Length), 0, 4);
                memoryStream.Write(binaryContent3, 0, binaryContent3.Length);
                length += 4 + binaryContent3.Length;

                multiMessageBuffer = memoryStream.GetBuffer();
            }

            var networkData = new NetworkData()
            {
                Buffer = multiMessageBuffer,
                Length = length,
                RemoteHost = NodeBuilder.BuildNode().Host("host1.com").WithPort(1000)
            };

            List<NetworkData> decodedMessages;
            Decoder.Decode(networkData, out decodedMessages);
            Assert.AreEqual(3, decodedMessages.Count);
            Assert.IsTrue(binaryContent1.SequenceEqual(decodedMessages[0].Buffer));
            Assert.IsTrue(binaryContent2.SequenceEqual(decodedMessages[1].Buffer));
            Assert.IsTrue(binaryContent3.SequenceEqual(decodedMessages[2].Buffer));
        }

        #endregion
    }
}
