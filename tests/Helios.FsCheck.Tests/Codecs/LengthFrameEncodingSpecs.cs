using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FsCheck;
using Microsoft.FSharp.Control;
using Xunit;
using FsCheck.Xunit;
using Helios.Buffers;
using Helios.Channels.Embedded;
using Helios.Codecs;
using Helios.FsCheck.Tests.Buffers;

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
            var partialReader = new PartialReadGenerator(new[] { new ReadInstruction(ReadMode.Partial, 5, 8),  });
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
            var inbound2 = ec.ReadInbound<IByteBuf>();
            Assert.Equal(byteBuff, inbound2, AbstractByteBuf.ByteBufComparer);
        }

        [Property(QuietOnSuccess = true, MaxTest = 10000)]
        public Property LengthFrameEncoders_should_correctly_encode_anything_LittleEndian(Tuple<IByteBuf, ReadInstruction>[] reads)
        {
            var expectedReads = reads.Select(x => x.Item1).ToArray();
            var readModes = reads.Select(x => x.Item2).ToArray();
            var partialReader = new PartialReadGenerator(readModes);
            var prepender = new LengthFieldPrepender(4, false);
            var decoder = new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4);
            var ec = new EmbeddedChannel(partialReader, prepender, decoder);
            
            var actualReads = new List<IByteBuf>();
            foreach (var read in reads)
            {
                ec.WriteOutbound(read.Item1.Duplicate());

                IByteBuf encoded;
                do
                {
                    encoded = ec.ReadOutbound<IByteBuf>();
                    if(encoded != null)
                        ec.WriteInbound(encoded);
                } while (encoded != null);
                
            }

            // do one final read in case the last readmode was a partial
            ec.Pipeline.FireUserEventTriggered(PartialReadGenerator.FinishPartialReads.Instance);
            IByteBuf decoded;
            do
            {
                decoded = ec.ReadInbound<IByteBuf>();
                if (decoded != null 
                    && decoded.ReadableBytes > 0) 
                {
                    actualReads.Add(decoded);
                }
            } while (decoded != null);

            return
                expectedReads.SequenceEqual(actualReads, AbstractByteBuf.ByteBufComparer)
                .When(reads.Length > 0 // need something to read
                && reads.All(x => x.Item1.ReadableBytes > 0 // length can't be zero
                && x.Item1.ReadableBytes != 4)) // length can't be equal to the length frame length
                    .Label($"Expected encoders and decoders to read each other's messages, even with partial reads. " +
                           $"Expected: {string.Join("|", expectedReads.Select(x => ByteBufferUtil.HexDump(x)))}" + Environment.NewLine +
                           $"Actual: {string.Join("|", actualReads.Select(x => ByteBufferUtil.HexDump(x)))}");
        }
    }
}
