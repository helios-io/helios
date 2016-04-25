using System.Diagnostics.Contracts;
using Helios.Buffers;
using Helios.Channels;

namespace Helios.Codecs
{
    /// <summary>
    /// Used for providing message framing capabilities for inbound data
    /// </summary>
    public abstract class ByteToMessageDecoder : ChannelHandlerAdapter
    {
        /// <summary>
        /// Cumulation function used for handling merging together <see cref="IByteBuf"/>s from partial reads
        /// </summary>
        /// <param name="alloc">The allocator used for creating / managing buffers</param>
        /// <param name="cumulation">The existing cumulative buffer</param>
        /// <param name="input">The input buffer from the current read</param>
        /// <returns>A combined cumulative buffer</returns>
        public delegate IByteBuf Cumulator(IByteBufAllocator alloc, IByteBuf cumulation, IByteBuf input);

        public static readonly Cumulator Merge = (alloc, cumulation, input) =>
        {
            IByteBuf buffer;
            buffer = cumulation.WriterIndex > cumulation.MaxCapacity - input.ReadableBytes ? 
                ExpandCumulation(alloc, cumulation, input.ReadableBytes) : // expand the buffer
                cumulation; // or, use the original since input will fit inside
            buffer.WriteBytes(input);
            // todo: referencing counting release
            return buffer;
        };

        /// <summary>
        /// Expand the existing cumulative <see cref="IByteBuf"/>.
        /// </summary>
        static IByteBuf ExpandCumulation(IByteBufAllocator alloc, IByteBuf cumulation, int readable)
        {
            var old = cumulation;
            cumulation = alloc.Buffer(old.ReadableBytes + readable);
            cumulation.WriteBytes(old);
            // todo: reference count the old cumulation buffer
            return cumulation;
        }

        protected IByteBuf Cumulation;
        private Cumulator _cumulator = Merge;
        private bool _singleDecode;
        private bool _decodeWasNull;
        private bool _first;
        private int _discardAfterReads = 16;
        private int _numReads;

        /// <summary>
        /// When set to <c>true</c> then only one message is decoded on each <see cref="IChannelHandler.ChannelRead"/>.
        /// 
        /// Defaults to <c>false</c> for performance reasons.
        /// </summary>
        /// <param name="singleDecode">The toggle for single decoding</param>
        /// <remarks>May be useful if oyu need to do a protocol upgrade and want to make sure nothing is mixed up.</remarks>
        public void SetSingleDecode(bool singleDecode)
        {
            _singleDecode = singleDecode;
        }

        public bool IsSingleDecode => _singleDecode;

        /// <summary>
        /// Set the <see cref="Cumulator"/> function used by this decoder.
        /// </summary>
        public void SetCumulator(Cumulator cumulator)
        {
            Contract.Requires(cumulator != null);
            _cumulator = cumulator;
        }

        /// <summary>
        /// Set the number of reads after whcih <see cref="IByteBuf.DiscardSomeReadBytes"/> are called to free up memory.
        /// 
        /// The default is <c>16</c>.
        /// </summary>
        /// <param name="discardAfterReads"></param>
        public void SetDiscardAfterReads(int discardAfterReads)
        {
            Contract.Requires(discardAfterReads > 0);
            _discardAfterReads = discardAfterReads;
        }
    }
}
