// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using Helios.Buffers;
using Helios.Channels;

namespace Helios.Codecs
{
    public class LengthFieldBasedFrameDecoder : ByteToMessageDecoder
    {
        private readonly ByteOrder byteOrder;
        private readonly bool failFast;
        private readonly int initialBytesToStrip;
        private readonly int lengthAdjustment;
        private readonly int lengthFieldEndOffset;
        private readonly int lengthFieldLength;
        private readonly int lengthFieldOffset;
        private readonly int maxFrameLength;
        private long bytesToDiscard;
        private bool discardingTooLongFrame;
        private long tooLongFrameLength;

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, 0, 0)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength,
            int lengthAdjustment, int initialBytesToStrip)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException" /> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength" /> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException" /> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength" /> has been read.
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public LengthFieldBasedFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength,
            int lengthAdjustment, int initialBytesToStrip, bool failFast)
            : this(
                ByteOrder.LittleEndian, maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment,
                initialBytesToStrip, failFast)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="byteOrder">The <see cref="ByteOrder" /> of the lenght field.</param>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException" /> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength" /> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException" /> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength" /> has been read.
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public LengthFieldBasedFrameDecoder(ByteOrder byteOrder, int maxFrameLength, int lengthFieldOffset,
            int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
        {
            if (maxFrameLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength),
                    "maxFrameLength must be a positive integer: " + maxFrameLength);
            }
            if (lengthFieldOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthFieldOffset),
                    "lengthFieldOffset must be a non-negative integer: " + lengthFieldOffset);
            }
            if (initialBytesToStrip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBytesToStrip),
                    "initialBytesToStrip must be a non-negative integer: " + initialBytesToStrip);
            }
            if (lengthFieldOffset > maxFrameLength - lengthFieldLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength),
                    "maxFrameLength (" + maxFrameLength + ") " +
                    "must be equal to or greater than " +
                    "lengthFieldOffset (" + lengthFieldOffset + ") + " +
                    "lengthFieldLength (" + lengthFieldLength + ").");
            }

            this.byteOrder = byteOrder;
            this.maxFrameLength = maxFrameLength;
            this.lengthFieldOffset = lengthFieldOffset;
            this.lengthFieldLength = lengthFieldLength;
            this.lengthAdjustment = lengthAdjustment;
            lengthFieldEndOffset = lengthFieldOffset + lengthFieldLength;
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
        ///     Create a frame out of the <see cref="IByteBuf" /> and return it.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="IChannelHandlerContext" /> which this <see cref="ByteToMessageDecoder" /> belongs
        ///     to.
        /// </param>
        /// <param name="input">The <see cref="IByteBuf" /> from which to read data.</param>
        /// <returns>The <see cref="IByteBuf" /> which represents the frame or <c>null</c> if no frame could be created.</returns>
        protected object Decode(IChannelHandlerContext context, IByteBuf input)
        {
            if (discardingTooLongFrame)
            {
                var bytesToDiscard = this.bytesToDiscard;
                var localBytesToDiscard = (int) Math.Min(bytesToDiscard, input.ReadableBytes);
                input.SkipBytes(localBytesToDiscard);
                bytesToDiscard -= localBytesToDiscard;
                this.bytesToDiscard = bytesToDiscard;

                FailIfNecessary(false);
            }

            if (input.ReadableBytes < lengthFieldEndOffset)
            {
                return null;
            }

            var actualLengthFieldOffset = input.ReaderIndex + lengthFieldOffset;
            var frameLength = GetUnadjustedFrameLength(input, actualLengthFieldOffset, lengthFieldLength, byteOrder);

            if (frameLength < 0)
            {
                input.SkipBytes(lengthFieldEndOffset);
                throw new CorruptedFrameException("negative pre-adjustment length field: " + frameLength);
            }

            frameLength += lengthAdjustment + lengthFieldEndOffset;

            if (frameLength < lengthFieldEndOffset)
            {
                input.SkipBytes(lengthFieldEndOffset);
                throw new CorruptedFrameException("Adjusted frame length (" + frameLength + ") is less " +
                                                  "than lengthFieldEndOffset: " + lengthFieldEndOffset);
            }

            if (frameLength > maxFrameLength)
            {
                var discard = frameLength - input.ReadableBytes;
                tooLongFrameLength = frameLength;

                if (discard < 0)
                {
                    // buffer contains more bytes then the frameLength so we can discard all now
                    input.SkipBytes((int) frameLength);
                }
                else
                {
                    // Enter the discard mode and discard everything received so far.
                    discardingTooLongFrame = true;
                    bytesToDiscard = discard;
                    input.SkipBytes(input.ReadableBytes);
                }
                FailIfNecessary(true);
                return null;
            }

            // never overflows because it's less than maxFrameLength
            var frameLengthInt = (int) frameLength;
            if (input.ReadableBytes < frameLengthInt)
            {
                return null;
            }

            if (initialBytesToStrip > frameLengthInt)
            {
                input.SkipBytes(frameLengthInt);
                throw new CorruptedFrameException("Adjusted frame length (" + frameLength + ") is less " +
                                                  "than initialBytesToStrip: " + initialBytesToStrip);
            }
            input.SkipBytes(initialBytesToStrip);

            // extract frame
            var readerIndex = input.ReaderIndex;
            var actualFrameLength = frameLengthInt - initialBytesToStrip;
            var frame = ExtractFrame(context, input, readerIndex, actualFrameLength);
            input.SetReaderIndex(readerIndex + actualFrameLength);
            return frame;
        }

        /// <summary>
        ///     Decodes the specified region of the buffer into an unadjusted frame length.  The default implementation is
        ///     capable of decoding the specified region into an unsigned 8/16/24/32/64 bit integer.  Override this method to
        ///     decode the length field encoded differently.
        ///     Note that this method must not modify the state of the specified buffer (e.g. <see cref="IByteBuf.ReaderIndex" />,
        ///     <see cref="IByteBuf.WriterIndex" />, and the content of the buffer.)
        /// </summary>
        /// <param name="buffer">The buffer we'll be extracting the frame length from.</param>
        /// <param name="offset">The offset from the absolute <see cref="IByteBuf.ReaderIndex" />.</param>
        /// <param name="length">The length of the framelenght field. Expected: 1, 2, 3, 4, or 8.</param>
        /// <param name="order">The preferred <see cref="ByteOrder" /> of <see cref="buffer" />.</param>
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
                    throw new DecoderException("unsupported lengthFieldLength: " + lengthFieldLength +
                                               " (expected: 1, 2, 3, 4, or 8)");
            }
            return frameLength;
        }

        protected virtual IByteBuf ExtractFrame(IChannelHandlerContext context, IByteBuf buffer, int index, int length)
        {
            var buff = buffer.Slice(index, length);
            buff.Retain();
            return buff;
        }

        private void FailIfNecessary(bool firstDetectionOfTooLongFrame)
        {
            if (bytesToDiscard == 0)
            {
                // Reset to the initial state and tell the handlers that
                // the frame was too large.
                var tooLongFrameLength = this.tooLongFrameLength;
                this.tooLongFrameLength = 0;
                discardingTooLongFrame = false;
                if (!failFast ||
                    failFast && firstDetectionOfTooLongFrame)
                {
                    Fail(tooLongFrameLength);
                }
            }
            else
            {
                // Keep discarding and notify handlers if necessary.
                if (failFast && firstDetectionOfTooLongFrame)
                {
                    Fail(tooLongFrameLength);
                }
            }
        }

        private void Fail(long frameLength)
        {
            if (frameLength > 0)
            {
                throw new TooLongFrameException("Adjusted frame length exceeds " + maxFrameLength +
                                                ": " + frameLength + " - discarded");
            }
            throw new TooLongFrameException(
                "Adjusted frame length exceeds " + maxFrameLength +
                " - discarding");
        }
    }
}