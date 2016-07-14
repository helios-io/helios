// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using Helios.Buffers;
using Helios.Codecs;
using Helios.Net;
using Helios.Tracing;

namespace Helios.Serialization
{
    /// <summary>
    ///     Decodes messages based off of a length frame added to the front of the message
    /// </summary>
    public class LengthFieldFrameBasedDecoder : MessageDecoderBase
    {
        private readonly bool _failFast;
        private readonly int _initialBytesToStrip;
        private readonly int _lengthAdjustment;
        private readonly int _lengthFieldEndOffset;
        private readonly int _lengthFieldLength;
        private readonly int _lengthFieldOffset;
        private readonly int _maxFrameLength;
        private long _bytesToDiscard;
        private bool _discardingTooLongFrame;
        private long _tooLongFrameLength;

        public LengthFieldFrameBasedDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, 0, 0)
        {
        }

        public LengthFieldFrameBasedDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength,
            int lengthAdjustment, int initialBytesToStrip)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true)
        {
        }

        public LengthFieldFrameBasedDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength,
            int lengthAdjustment, int initialBytesToStrip, bool failFast)
        {
            _maxFrameLength = maxFrameLength;
            _lengthFieldOffset = lengthFieldOffset;
            _lengthFieldLength = lengthFieldLength;
            _lengthFieldEndOffset = lengthFieldLength + lengthFieldOffset;
            _lengthAdjustment = lengthAdjustment;
            _initialBytesToStrip = initialBytesToStrip;
            _failFast = failFast;
        }

        #region Static methods

        /// <summary>
        ///     Returns a default <see cref="LengthFieldFrameBasedDecoder" /> that uses a 4-byte header to describe the length of a
        ///     frame
        ///     of up to 128k in size.
        /// </summary>
        public static LengthFieldFrameBasedDecoder Default
        {
            get { return new LengthFieldFrameBasedDecoder(128000, 0, 4, 0, 4, true); }
        }

        #endregion

        public override void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded)
        {
            decoded = new List<IByteBuf>();
            var obj = Decode(connection, buffer);
            while (obj != null)
            {
                decoded.Add(obj);
                HeliosTrace.Instance.DecodeSucccess(1);
                obj = Decode(connection, buffer);
            }
        }

        public override IMessageDecoder Clone()
        {
            return new LengthFieldFrameBasedDecoder(_maxFrameLength, _lengthFieldOffset, _lengthFieldLength,
                _lengthAdjustment, _initialBytesToStrip, _failFast);
        }

        protected IByteBuf Decode(IConnection connection, IByteBuf input)
        {
            if (_discardingTooLongFrame)
            {
                var bytesToDiscard = _bytesToDiscard;
                var localBytesToDiscard = (int) Math.Min(bytesToDiscard, input.ReadableBytes);
                input.SkipBytes(localBytesToDiscard);
                bytesToDiscard -= localBytesToDiscard;
                _bytesToDiscard = bytesToDiscard;
                FailIfNecessary(false);
            }

            if (input.ReadableBytes < _lengthFieldEndOffset) return null;

            var actualLengthFieldOffset = input.ReaderIndex + _lengthFieldOffset;
            var frameLength = GetUnadjustedFrameLength(input, actualLengthFieldOffset, _lengthFieldLength);

            if (frameLength < 0)
            {
                input.SkipBytes(_lengthFieldEndOffset);
                throw new CorruptedFrameException(
                    string.Format("negative or zero pre-adjustment length field: " + frameLength));
            }

            frameLength += _lengthAdjustment + _lengthFieldEndOffset;

            if (frameLength < _lengthFieldEndOffset)
            {
                input.SkipBytes(_lengthFieldEndOffset);
                throw new CorruptedFrameException(
                    string.Format("Adjusted frame length ({0}) is less than lengthFieldEndOffset: {1}", frameLength,
                        _lengthFieldEndOffset));
            }

            if (frameLength > _maxFrameLength)
            {
                var discard = frameLength - input.ReadableBytes;
                _tooLongFrameLength = frameLength;

                if (discard < 0)
                {
                    // buffer contains more bytes than the frameLength so we can discard all now
                    input.SkipBytes((int) frameLength);
                    HeliosTrace.Instance.DecodeMalformedBytes((int) frameLength);
                }
                else
                {
                    //Enter discard mode and discard everything receive so far
                    _discardingTooLongFrame = true;
                    _bytesToDiscard = discard;
                    HeliosTrace.Instance.DecodeMalformedBytes(input.ReadableBytes);
                    input.SkipBytes(input.ReadableBytes);
                }
                FailIfNecessary(true);
                return null;
            }

            // never overflows because it's less than _maxFrameLength
            var frameLengthInt = (int) frameLength;
            if (input.ReadableBytes < frameLengthInt)
            {
                var unreadBytes = new byte[input.ReadableBytes];
                input.GetBytes(input.ReaderIndex, unreadBytes, 0, input.ReadableBytes);
                //need additional data from the network before we can finish decoding this message
                return null;
            }

            if (_initialBytesToStrip > frameLengthInt)
            {
                input.SkipBytes(frameLengthInt);
                HeliosTrace.Instance.DecodeFailure();
                throw new CorruptedFrameException(
                    string.Format("Adjusted frame lenght ({0}) is less than initialBytesToStrip: {1}", frameLength,
                        _initialBytesToStrip));
            }
            input.SkipBytes(_initialBytesToStrip);

            //extract frame
            var readerIndex = input.ReaderIndex;
            var actualFrameLength = frameLengthInt - _initialBytesToStrip;
            var frame = ExtractFrame(connection, input, readerIndex, actualFrameLength);
            input.SetReaderIndex(readerIndex + actualFrameLength);
            return frame;
        }

        protected IByteBuf ExtractFrame(IConnection connection, IByteBuf buffer, int index, int length)
        {
            var frame = connection.Allocator.Buffer(length);
            frame.WriteBytes(buffer, index, length);
            return frame;
        }

        protected long GetUnadjustedFrameLength(IByteBuf buf, int offset, int length)
        {
            long framelength;
            switch (length)
            {
                case 1:
                    framelength = buf.GetByte(offset);
                    break;
                case 2:
                    framelength = buf.GetShort(offset);
                    break;
                case 4:
                    framelength = buf.GetInt(offset);
                    break;
                case 8:
                    framelength = buf.GetLong(offset);
                    break;
                default:
                    throw new DecoderException(
                        string.Format("unsupported lengtFieldLength: {0} (expected: 1, 2, 4, or 8)", length));
            }
            return framelength;
        }

        protected void FailIfNecessary(bool firstDetectionOfTooLongFrame)
        {
            if (_bytesToDiscard == 0)
            {
                // Reset to the initial state and tell the handlers that the
                // frame was too large
                var tooLongFrameLength = _tooLongFrameLength;
                _tooLongFrameLength = 0;
                _discardingTooLongFrame = false;
                if (!_failFast || (_failFast && firstDetectionOfTooLongFrame))
                {
                    Fail(_tooLongFrameLength);
                }
                else
                {
                    // Keep discarding and notify handlers if necessary
                    if (_failFast && firstDetectionOfTooLongFrame)
                    {
                        Fail(tooLongFrameLength);
                    }
                }
            }
        }

        private void Fail(long frameLength)
        {
            try
            {
                if (frameLength > 0)
                    throw new TooLongFrameException(string.Format("Adjusted frame length exceeds {0}: {1} - discarded",
                        _maxFrameLength, frameLength));
                throw new TooLongFrameException(string.Format("Adjusted frame lenght exceeds {0} - discarding",
                    _maxFrameLength));
            }
            catch
            {
                HeliosTrace.Instance.DecodeFailure();
                throw;
            }
        }
    }
}