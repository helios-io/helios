using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;

namespace Helios.Codecs
{
    public class LengthFieldBasedFrameDecoder : ByteToMessageDecoder
    {
        readonly ByteOrder byteOrder;
        readonly int maxFrameLength;
        readonly int lengthFieldOffset;
        readonly int lengthFieldLength;
        readonly int lengthFieldEndOffset;
        readonly int lengthAdjustment;
        readonly int initialBytesToStrip;
        readonly bool failFast;
        bool discardingTooLongFrame;
        long tooLongFrameLength;
        long bytesToDiscard;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">The maximum length of the frame.  If the length of the frame is
        /// greater than this value then <see cref="TooLongFrameException"/> will be thrown.</param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, 0, 0)
        {
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">The maximum length of the frame.  If the length of the frame is
        /// greater than this value then <see cref="TooLongFrameException"/> will be thrown.</param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true)
        {
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">The maximum length of the frame.  If the length of the frame is
        /// greater than this value then <see cref="TooLongFrameException"/> will be thrown.</param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException"/> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength"/> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException"/> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength"/> has been read.
        /// 
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
            : this(ByteOrder.LittleEndian, maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, failFast)
        {
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="byteOrder">The <see cref="ByteOrder"/> of the lenght field.</param>
        /// <param name="maxFrameLength">The maximum length of the frame.  If the length of the frame is
        /// greater than this value then <see cref="TooLongFrameException"/> will be thrown.</param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException"/> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength"/> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException"/> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength"/> has been read.
        /// 
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public LengthFieldBasedFrameDecoder(ByteOrder byteOrder, int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
        {
            if (maxFrameLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength must be a positive integer: " + maxFrameLength);
            }
            if (lengthFieldOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthFieldOffset), "lengthFieldOffset must be a non-negative integer: " + lengthFieldOffset);
            }
            if (initialBytesToStrip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBytesToStrip), "initialBytesToStrip must be a non-negative integer: " + initialBytesToStrip);
            }
            if (lengthFieldOffset > maxFrameLength - lengthFieldLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength (" + maxFrameLength + ") " +
                    "must be equal to or greater than " +
                    "lengthFieldOffset (" + lengthFieldOffset + ") + " +
                    "lengthFieldLength (" + lengthFieldLength + ").");
            }

            this.byteOrder = byteOrder;
            this.maxFrameLength = maxFrameLength;
            this.lengthFieldOffset = lengthFieldOffset;
            this.lengthFieldLength = lengthFieldLength;
            this.lengthAdjustment = lengthAdjustment;
            this.lengthFieldEndOffset = lengthFieldOffset + lengthFieldLength;
            this.initialBytesToStrip = initialBytesToStrip;
            this.failFast = failFast;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output)
        {
            var decoded = Decode(context, input);
            if (decoded != null)
            {
                output.Add(decoded);
            }
        }

        /// <summary>
        /// Create a frame out of the <see cref="IByteBuf"/> and return it.
        /// </summary>
        /// <param name="context">The <see cref="IChannelHandlerContext"/> which this <see cref="ByteToMessageDecoder"/> belongs to.</param>
        /// <param name="input">The <see cref="IByteBuf"/> from which to read data.</param>
        /// <returns>The <see cref="IByteBuf"/> which represents the frame or <c>null</c> if no frame could be created.</returns>
        protected object Decode(IChannelHandlerContext context, IByteBuf input)
        {
            if (this.discardingTooLongFrame)
            {
                long bytesToDiscard = this.bytesToDiscard;
                int localBytesToDiscard = (int)Math.Min(bytesToDiscard, input.ReadableBytes);
                input.SkipBytes(localBytesToDiscard);
                bytesToDiscard -= localBytesToDiscard;
                this.bytesToDiscard = bytesToDiscard;

                this.FailIfNecessary(false);
            }

            if (input.ReadableBytes < this.lengthFieldEndOffset)
            {
                return null;
            }

            int actualLengthFieldOffset = input.ReaderIndex + this.lengthFieldOffset;
            long frameLength = this.GetUnadjustedFrameLength(input, actualLengthFieldOffset, this.lengthFieldLength, this.byteOrder);

            if (frameLength < 0)
            {
                input.SkipBytes(this.lengthFieldEndOffset);
                throw new CorruptedFrameException("negative pre-adjustment length field: " + frameLength);
            }

            frameLength += this.lengthAdjustment + this.lengthFieldEndOffset;

            if (frameLength < this.lengthFieldEndOffset)
            {
                input.SkipBytes(this.lengthFieldEndOffset);
                throw new CorruptedFrameException("Adjusted frame length (" + frameLength + ") is less " +
                    "than lengthFieldEndOffset: " + this.lengthFieldEndOffset);
            }

            if (frameLength > this.maxFrameLength)
            {
                long discard = frameLength - input.ReadableBytes;
                this.tooLongFrameLength = frameLength;

                if (discard < 0)
                {
                    // buffer contains more bytes then the frameLength so we can discard all now
                    input.SkipBytes((int)frameLength);
                }
                else
                {
                    // Enter the discard mode and discard everything received so far.
                    this.discardingTooLongFrame = true;
                    this.bytesToDiscard = discard;
                    input.SkipBytes(input.ReadableBytes);
                }
                this.FailIfNecessary(true);
                return null;
            }

            // never overflows because it's less than maxFrameLength
            int frameLengthInt = (int)frameLength;
            if (input.ReadableBytes < frameLengthInt)
            {
                return null;
            }

            if (this.initialBytesToStrip > frameLengthInt)
            {
                input.SkipBytes(frameLengthInt);
                throw new CorruptedFrameException("Adjusted frame length (" + frameLength + ") is less " +
                    "than initialBytesToStrip: " + this.initialBytesToStrip);
            }
            input.SkipBytes(this.initialBytesToStrip);

            // extract frame
            int readerIndex = input.ReaderIndex;
            int actualFrameLength = frameLengthInt - this.initialBytesToStrip;
            IByteBuf frame = this.ExtractFrame(context, input, readerIndex, actualFrameLength);
            input.SetReaderIndex(readerIndex + actualFrameLength);
            return frame;
        }

        /// <summary>
        ///  Decodes the specified region of the buffer into an unadjusted frame length.  The default implementation is
        /// capable of decoding the specified region into an unsigned 8/16/24/32/64 bit integer.  Override this method to
        /// decode the length field encoded differently. 
        /// Note that this method must not modify the state of the specified buffer (e.g. <see cref="IByteBuf.ReaderIndex"/>, 
        /// <see cref="IByteBuf.WriterIndex"/>, and the content of the buffer.)
        /// </summary>
        /// <param name="buffer">The buffer we'll be extracting the frame length from.</param>
        /// <param name="offset">The offset from the absolute <see cref="IByteBuf.ReaderIndex"/>.</param>
        /// <param name="length">The length of the framelenght field. Expected: 1, 2, 3, 4, or 8.</param>
        /// <param name="order">The preferred <see cref="ByteOrder"/> of <see cref="buffer"/>.</param>
        /// <returns>A long integer that represents the unadjusted length of the next frame.</returns>
        protected long GetUnadjustedFrameLength(IByteBuf buffer, int offset, int length, ByteOrder order)
        {
            buffer = buffer.WithOrder(order);
            long frameLength;
            switch (length)
            {
                case 1:
                    frameLength = buffer.GetByte(offset);
                    break;
                case 2:
                    frameLength = buffer.GetShort(offset);
                    break;
                case 4:
                    frameLength = buffer.GetInt(offset);
                    break;
                case 8:
                    frameLength = buffer.GetLong(offset);
                    break;
                default:
                    throw new DecoderException("unsupported lengthFieldLength: " + this.lengthFieldLength + " (expected: 1, 2, 3, 4, or 8)");
            }
            return frameLength;
        }

        protected virtual IByteBuf ExtractFrame(IChannelHandlerContext context, IByteBuf buffer, int index, int length)
        {
            var buff = buffer.Slice(index, length);
            // buff.Retain(); // todo: reference counting
            return buff;
        }

        void FailIfNecessary(bool firstDetectionOfTooLongFrame)
        {
            if (this.bytesToDiscard == 0)
            {
                // Reset to the initial state and tell the handlers that
                // the frame was too large.
                long tooLongFrameLength = this.tooLongFrameLength;
                this.tooLongFrameLength = 0;
                this.discardingTooLongFrame = false;
                if (!this.failFast ||
                    this.failFast && firstDetectionOfTooLongFrame)
                {
                    this.Fail(tooLongFrameLength);
                }
            }
            else
            {
                // Keep discarding and notify handlers if necessary.
                if (this.failFast && firstDetectionOfTooLongFrame)
                {
                    this.Fail(this.tooLongFrameLength);
                }
            }
        }

        void Fail(long frameLength)
        {
            if (frameLength > 0)
            {
                throw new TooLongFrameException("Adjusted frame length exceeds " + this.maxFrameLength +
                    ": " + frameLength + " - discarded");
            }
            else
            {
                throw new TooLongFrameException(
                    "Adjusted frame length exceeds " + this.maxFrameLength +
                        " - discarding");
            }
        }
    }
}
