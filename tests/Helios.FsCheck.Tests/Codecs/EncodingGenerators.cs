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
        public static Arbitrary<Tuple<IByteBuf, ReadMode>> ChannelReads()
        {
            Func<IByteBuf, ReadMode, Tuple<IByteBuf, ReadMode>> producer = (buf, mode) => new Tuple<IByteBuf, ReadMode>(buf, mode);
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

        public PartialReadGenerator(ReadMode[] modes)
        {
            _modes = new Queue<ReadMode>(modes);
        }

        private readonly Queue<ReadMode> _modes;

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuf)
            {
                ReadMode mode;
                mode = _modes.Any() ? _modes.Dequeue() : ReadMode.Full;
                var buf = (IByteBuf)message;
                if (mode == ReadMode.Full)
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
                    var partialReadBytes = ThreadLocalRandom.Current.Next(5, buf.ReadableBytes / 2); // read up to half of the current message
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
