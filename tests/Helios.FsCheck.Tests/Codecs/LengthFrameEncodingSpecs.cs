// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Helios.Buffers;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Helios.FsCheck.Tests.Buffers;
using Xunit;

namespace Helios.FsCheck.Tests.Codecs
{
    public class LengthFrameEncodingSpecs
    {
        public LengthFrameEncodingSpecs()
        {
            Arb.Register<BufferGenerators>();
            Arb.Register<EncodingGenerators>();
        }

        [Fact]
        public void PartialReadGenerator_will_create_partial_read()
        {
            var prepender = new LengthFieldPrepender(4, false);
            var decoder = new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4);
            var partialReader = new PartialReadGenerator(new[] {new ReadInstruction(ReadMode.Partial, 5, 8)});
            var ec = new EmbeddedChannel(partialReader, prepender, decoder);
            var byteBuff = Unpooled.Buffer(10).WriteInt(4).WriteInt(10);
            ec.WriteAndFlushAsync(byteBuff.Duplicate()).Wait();

            IByteBuf encoded;
            do
            {
                encoded = ec.ReadOutbound<IByteBuf>();
                if (encoded != null)
                    ec.WriteInbound(encoded);
            } while (encoded != null);


            // finish the read
            ec.Pipeline.FireUserEventTriggered(PartialReadGenerator.FinishPartialReads.Instance);
            var inboud = ec.ReadInbound<IByteBuf>();
            Assert.Equal(byteBuff.ResetReaderIndex(), inboud, AbstractByteBuf.ByteBufComparer);
        }

        [Fact]
        public void PartialReadGenerator_will_correctly_handle_multiple_reads()
        {
            var prepender = new LengthFieldPrepender(4, false);
            var decoder = new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4);
            var partialReader =
                new PartialReadGenerator(new[]
                {
                    new ReadInstruction(ReadMode.Partial, 2, 8), new ReadInstruction(ReadMode.Partial, 4, 8),
                    new ReadInstruction(ReadMode.Full, 8, 8)
                });
            var ec = new EmbeddedChannel(partialReader, prepender, decoder);
            var byteBuf1 = Unpooled.Buffer(10).WriteInt(4).WriteInt(10);
            var byteBuf2 = Unpooled.Buffer(100).WriteInt(5).WriteInt(6).WriteBytes(new byte[92]);
            var byteBuf3 = Unpooled.Buffer(10).WriteInt(21).WriteInt(11);
            Task.WaitAll(ec.WriteAndFlushAsync(byteBuf1.Duplicate().Retain()),
                ec.WriteAndFlushAsync(byteBuf2.Duplicate().Retain()),
                ec.WriteAndFlushAsync(byteBuf3.Duplicate().Retain()));

            IByteBuf encoded;
            do
            {
                encoded = ec.ReadOutbound<IByteBuf>();
                if (encoded != null)
                    ec.WriteInbound(encoded.Duplicate().Retain());
            } while (encoded != null);


            // finish the read
            ec.Pipeline.FireUserEventTriggered(PartialReadGenerator.FinishPartialReads.Instance);
            var inbound1 = ec.ReadInbound<IByteBuf>();
            var inbound2 = ec.ReadInbound<IByteBuf>();
            var inbound3 = ec.ReadInbound<IByteBuf>();
            Assert.Equal(byteBuf1.ResetReaderIndex(), inbound1, AbstractByteBuf.ByteBufComparer);
            Assert.Equal(byteBuf2.ResetReaderIndex(), inbound2, AbstractByteBuf.ByteBufComparer);
            Assert.Equal(byteBuf3.ResetReaderIndex(), inbound3, AbstractByteBuf.ByteBufComparer);
            byteBuf1.Release();
            byteBuf2.Release();
            byteBuf3.Release();
        }

        [Property(MaxTest = 5000)]
        public Property LengthFrameEncoders_should_correctly_encode_anything_LittleEndian(
            Tuple<IByteBuf, ReadInstruction>[] reads)
        {
            var expectedReads = reads.Select(x => x.Item1).ToArray();
            var readModes = reads.Select(x => x.Item2).ToArray();
            var partialReader = new PartialReadGenerator(readModes);
            var prepender = new LengthFieldPrepender(4, false);
            var decoder = new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4);
            var ec = new EmbeddedChannel(partialReader, prepender, decoder);


            foreach (var read in expectedReads)
            {
                ec.WriteAndFlushAsync(read.Duplicate()).Wait();

                IByteBuf encoded;
                do
                {
                    encoded = ec.ReadOutbound<IByteBuf>();
                    if (encoded != null)
                        ec.WriteInbound(encoded);
                } while (encoded != null);
            }

            var actualReads = new List<IByteBuf>();
            // do one final read in case the last readmode was a partial
            ec.Pipeline.FireUserEventTriggered(PartialReadGenerator.FinishPartialReads.Instance);
            IByteBuf decoded;
            do
            {
                decoded = ec.ReadInbound<IByteBuf>();
                if (decoded != null)
                {
                    actualReads.Add(decoded);
                }
            } while (decoded != null);
            expectedReads = expectedReads.Select(x => x.ResetReaderIndex()).ToArray();
                // need to perform a read reset of the buffer
            var pass = expectedReads.SequenceEqual(actualReads, AbstractByteBuf.ByteBufComparer);

            if (!pass)
            {
                Debugger.Break();
            }

            return
                pass
                    .When(reads.Length > 0 // need something to read
                    )
                    .Label($"Expected encoders and decoders to read each other's messages, even with partial reads. " +
                           $"Expected: {string.Join("|", expectedReads.Select(x => ByteBufferUtil.HexDump(x)))}" +
                           Environment.NewLine +
                           $"Actual: {string.Join("|", actualReads.Select(x => ByteBufferUtil.HexDump(x)))}");
        }

        [Property(MaxTest = 1000)]
        public Property HeliosBackwardsCompatibilityLengthFrameEncoders_should_correctly_encode_anything_LittleEndian(
            Tuple<IByteBuf, ReadInstruction>[] reads)
        {
            var expectedReads = reads.Select(x => x.Item1).ToArray();
            var readModes = reads.Select(x => x.Item2).ToArray();
            var partialReader = new PartialReadGenerator(readModes);
            var prepender = new HeliosBackwardsCompatabilityLengthFramePrepender(4, false);
            var decoder = new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4);
            var ec = new EmbeddedChannel(partialReader, prepender, decoder);


            foreach (var read in expectedReads)
            {
                var task = ec.WriteAndFlushAsync(read.Duplicate());
                task.Wait();

                IByteBuf encoded;
                do
                {
                    encoded = ec.ReadOutbound<IByteBuf>();
                    if (encoded != null)
                        ec.WriteInbound(encoded);
                } while (encoded != null);
            }

            var actualReads = new List<IByteBuf>();
            // do one final read in case the last readmode was a partial
            ec.Pipeline.FireUserEventTriggered(PartialReadGenerator.FinishPartialReads.Instance);
            IByteBuf decoded;
            do
            {
                decoded = ec.ReadInbound<IByteBuf>();
                if (decoded != null)
                {
                    actualReads.Add(decoded);
                }
            } while (decoded != null);
            expectedReads = expectedReads.Select(x => x.ResetReaderIndex()).ToArray();
                // need to perform a read reset of the buffer
            var pass = expectedReads.SequenceEqual(actualReads, AbstractByteBuf.ByteBufComparer);

            if (!pass)
            {
                Debugger.Break();
            }

            return
                pass
                    .When(reads.Length > 0 // need something to read
                    )
                    .Label($"Expected encoders and decoders to read each other's messages, even with partial reads. " +
                           $"Expected: {string.Join("|", expectedReads.Select(x => ByteBufferUtil.HexDump(x)))}" +
                           Environment.NewLine +
                           $"Actual: {string.Join("|", actualReads.Select(x => ByteBufferUtil.HexDump(x)))}");
        }
    }
}

