// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helios.Buffers;
using Helios.Codecs;
using Helios.Net;
using Helios.Serialization;
using Xunit;
using LengthFieldPrepender = Helios.Serialization.LengthFieldPrepender;

namespace Helios.Tests.Serialization
{
    public class LengthFieldEncodingTests
    {
        #region Setup / Teardown

        protected int LengthFieldLength = 4;
        protected IMessageEncoder Encoder;
        protected IMessageDecoder Decoder;
        protected IConnection TestConnection = new DummyConnection(UnpooledByteBufAllocator.Default);

        public LengthFieldEncodingTests()
        {
            Encoder = new LengthFieldPrepender(LengthFieldLength);
            Decoder = new LengthFieldFrameBasedDecoder(2000, 0, LengthFieldLength, 0, LengthFieldLength); //stip headers
        }

        #endregion

        #region Tests

        [Fact]
        public void Should_encode_length_in_message()
        {
            var binaryContent = Encoding.UTF8.GetBytes("somebytes");
            var expectedBytes = binaryContent.Length;
            var data = Unpooled.Buffer(expectedBytes).WriteBytes(binaryContent);

            List<IByteBuf> encodedMessages;
            Encoder.Encode(TestConnection, data, out encodedMessages);

            var encodedData = encodedMessages[0];
            Assert.Equal(expectedBytes + 4, encodedData.ReadableBytes);
            var decodedLength = encodedData.ReadInt();
            Assert.Equal(expectedBytes, decodedLength);
        }

        [Fact]
        public void Should_decode_single_lengthFramed_message()
        {
            var binaryContent = Encoding.UTF8.GetBytes("somebytes");
            var expectedBytes = binaryContent.Length;

            var data = Unpooled.Buffer(expectedBytes).WriteBytes(binaryContent);

            List<IByteBuf> encodedMessages;
            Encoder.Encode(TestConnection, data, out encodedMessages);

            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, encodedMessages[0], out decodedMessages);

            Assert.True(binaryContent.SequenceEqual(decodedMessages[0].ToArray()));
        }

        [Fact]
        public void Should_decoded_multiple_lengthFramed_messages()
        {
            var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
            var binaryContent2 = Encoding.UTF8.GetBytes("moarbytes");
            var binaryContent3 = BitConverter.GetBytes(100034034L);

            var buffer = Unpooled.Buffer(100).WriteInt(binaryContent1.Length)
                .WriteBytes(binaryContent1).WriteInt(binaryContent2.Length).WriteBytes(binaryContent2)
                .WriteInt(binaryContent3.Length).WriteBytes(binaryContent3);


            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, buffer, out decodedMessages);
            Assert.Equal(3, decodedMessages.Count);
            Assert.True(binaryContent1.SequenceEqual(decodedMessages[0].ToArray()));
            Assert.True(binaryContent2.SequenceEqual(decodedMessages[1].ToArray()));
            Assert.True(binaryContent3.SequenceEqual(decodedMessages[2].ToArray()));
        }

        [Fact]
        public void Should_throw_exception_when_decoding_negative_frameLength()
        {
            Assert.Throws<CorruptedFrameException>(() =>
            {
                var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
                var buffer = Unpooled.Buffer(100)
                    .WriteInt(-1*binaryContent1.Length) //make the frame length negative
                    .WriteBytes(binaryContent1);

                List<IByteBuf> decodedMessages;
                Decoder.Decode(TestConnection, buffer, out decodedMessages);
            });
        }

        [Fact]
        public void Should_throw_exception_when_decoding_zero_frameLength()
        {
            var binaryContent1 = Encoding.UTF8.GetBytes("somebytes");
            var buffer = Unpooled.Buffer(100)
                .WriteInt(0) //make the frame length negative
                .WriteBytes(binaryContent1);

            List<IByteBuf> decodedMessages;
            Decoder.Decode(TestConnection, buffer, out decodedMessages);
            //Assert.Pass("Helios should be in byte disacarding mode now");
        }

        #endregion
    }
}