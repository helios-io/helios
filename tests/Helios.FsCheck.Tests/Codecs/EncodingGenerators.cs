using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FsCheck;
using Helios.Buffers;
using Helios.Channels;
using Helios.FsCheck.Tests.Buffers;
using Helios.Util;
using Xunit;
using static Helios.FsCheck.Tests.FsharpDelegateHelper;

namespace Helios.FsCheck.Tests.Codecs
{
    public class EncodingGenerators
    {
        public static Arbitrary<Tuple<IByteBuf, ReadInstruction>> ChannelReads()
        {
            Func<IByteBuf, ReadMode, Tuple<IByteBuf, ReadInstruction>> producer = (buf, mode) =>
            {
                if (mode == ReadMode.Full)
                    return new Tuple<IByteBuf, ReadInstruction>(buf,
                        new ReadInstruction(mode, buf.ReadableBytes, buf.ReadableBytes));
                var partialBytesToRead = ThreadLocalRandom.Current.Next(1, buf.ReadableBytes-1); // TODO: find a way to use Gen here
                return new Tuple<IByteBuf, ReadInstruction>(buf,
                        new ReadInstruction(mode, partialBytesToRead, buf.ReadableBytes));
            };
            var fsFunc = Create(producer);
            return Arb.From(Gen.Map2(fsFunc, BufferGenerators.ByteBuf().Generator, GenReadMode()));
        }

        public static readonly ReadMode[] AllReadModes = Enum.GetValues(typeof(ReadMode)).Cast<ReadMode>().ToArray();

        public static Gen<ReadMode> GenReadMode()
        {
            return Gen.Elements(AllReadModes);
        }
    }

    public enum ReadMode
    {
        Full,
        Partial
    };

    public struct ReadInstruction
    {
        public ReadInstruction(ReadMode mode, int readBytes, int fullBytes)
        {
            Mode = mode;
            ReadBytes = readBytes;
            FullBytes = fullBytes;
        }

        public ReadMode Mode { get; private set; }
        public int ReadBytes { get; private set; }

        public int FullBytes { get; private set; }

        public override string ToString()
        {
            return $"ReadInstruction(Mode={Mode}, ReadBytes={ReadBytes}, FullBytes={FullBytes}}}";
        }

        public static readonly ReadInstruction Full = new ReadInstruction(ReadMode.Full, 0, 0);
    }

    /// <summary>
    /// A <see cref="ChannelHandlerAdapter"/> that will randomly 
    /// </summary>
    public class PartialReadGenerator : ChannelHandlerAdapter
    {
        public class FinishPartialReads
        {
            private FinishPartialReads() { }

            public static readonly FinishPartialReads Instance = new FinishPartialReads();
        }

        private IByteBuf _cumulativeBuffer = Unpooled.Buffer(4096);

        public PartialReadGenerator(ReadInstruction[] instructions)
        {
            _instructions = new Queue<ReadInstruction>(instructions);
        }

        private readonly Queue<ReadInstruction> _instructions;

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuf)
            {
                ReadInstruction mode;
                var buf = (IByteBuf)message;
                mode = _instructions.Any() && buf.ReadableBytes > 4 ? _instructions.Dequeue() : ReadInstruction.Full;
                
                if (mode.Mode == ReadMode.Full)
                {
                    var writeBuf =
                        context.Allocator.Buffer(_cumulativeBuffer.ReadableBytes + buf.ReadableBytes)
                            .WriteBytes(_cumulativeBuffer)
                            .WriteBytes(buf);
                    Assert.Equal(0, _cumulativeBuffer.ReadableBytes); // verify that we've fully drained the cumulative buffer
                    _cumulativeBuffer.DiscardReadBytes();
                    context.FireChannelRead(writeBuf);
                }
                else
                {
                    var originalBytes = buf.ReadableBytes;
                    var partialReadBytes = mode.ReadBytes;
                    var writeBuf =
                        context.Allocator.Buffer(_cumulativeBuffer.ReadableBytes + partialReadBytes)
                            .WriteBytes(_cumulativeBuffer)
                            .WriteBytes(buf, partialReadBytes);
                    Assert.Equal(0, _cumulativeBuffer.ReadableBytes); // verify that we've fully drained the cumulative buffer
                    _cumulativeBuffer.DiscardReadBytes();

                    // store the rest partial read into the cumulative buffer
                    _cumulativeBuffer.WriteBytes(buf, partialReadBytes, buf.ReadableBytes);
                    Assert.Equal(partialReadBytes + _cumulativeBuffer.ReadableBytes, originalBytes);

                    context.FireChannelRead(writeBuf);
                }
            }
            else
            {
                // move onto the next handler if this message is not an IByteBuf
                context.FireChannelRead(message);
            }
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is FinishPartialReads)
            {
                if (_cumulativeBuffer.IsReadable())
                {
                    var writeBuf =
                   context.Allocator.Buffer(_cumulativeBuffer.ReadableBytes)
                       .WriteBytes(_cumulativeBuffer);
                    context.FireChannelRead(writeBuf);
                }
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            // todo: release cumulative buffer
            context.FireChannelInactive();
        }
    }
}
