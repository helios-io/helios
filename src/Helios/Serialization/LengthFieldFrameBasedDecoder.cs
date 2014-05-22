using System;
using System.Collections.Generic;
using Helios.Net;
using Helios.Topology;

namespace Helios.Serialization
{
    /// <summary>
    /// Decodes messages based off of a length frame added to the front of the message
    /// </summary>
    public class LengthFieldFrameBasedDecoder : MessageDecoderBase
    {
        private readonly int _maxFrameLength;
        private readonly int _lengthFieldOffset;
        private readonly int _lengthFieldLength;
        private readonly int _lengthAdjustment;
        private readonly int _initialBytesToStrip;
        private readonly bool _failFast;

        public LengthFieldFrameBasedDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength) : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, 0, 0)
        {
        }

        public LengthFieldFrameBasedDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true)
        {
        }

        public LengthFieldFrameBasedDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
        {
            _maxFrameLength = maxFrameLength;
            _lengthFieldOffset = lengthFieldOffset;
            _lengthFieldLength = lengthFieldLength;
            _lengthAdjustment = lengthAdjustment;
            _initialBytesToStrip = initialBytesToStrip;
            _failFast = failFast;
        }

        public override void Decode(NetworkData data, out List<NetworkData> decoded)
        {
            decoded = new List<NetworkData>();
            var position = 0;
            while (position < data.Length)
            {
                NetworkData nextFrame;
                position = Decode(data, position, out nextFrame);
                if(!nextFrame.RemoteHost.IsEmpty())
                    decoded.Add(nextFrame);
            }
        }

        protected int Decode(NetworkData input, int initialOffset, out NetworkData nextFrame)
        {
            nextFrame = NetworkData.Empty;
            if (input.Length < _lengthFieldOffset) return input.Length;

            var actualLengthFieldOffset = initialOffset + _lengthFieldOffset;
            var frameLength = GetFrameLength(input, actualLengthFieldOffset, _lengthFieldLength);

            if (frameLength > _maxFrameLength)
            {
                if(_failFast) throw new Exception(string.Format("Object exceeded maximum length of {0} bytes. Was {1}", _maxFrameLength, frameLength));
                return input.Length;
            }

            var frameLengthInt = (int) frameLength;
            if (input.Length < frameLengthInt)
            {
                return input.Length;
            }

            //extract the framed message
            var index = _lengthFieldLength + actualLengthFieldOffset;
            var actualFrameLength = frameLengthInt - _initialBytesToStrip;
            nextFrame = ExtractFrame(input, index, actualFrameLength);
            return index + actualFrameLength;
        }

        protected long GetFrameLength(NetworkData data, int offset, int length)
        {
            long frameLength = 0;
            switch (length)
            {
                case 1:
                    frameLength = data.Buffer[offset];
                    break;
                case 2:
                    frameLength = BitConverter.ToUInt16(data.Buffer, offset);
                    break;
                case 4:
                    frameLength = BitConverter.ToUInt32(data.Buffer, offset);
                    break;
                case 8:
                    frameLength = BitConverter.ToInt64(data.Buffer, offset);
                    break;
                default:
                    throw new ArgumentException(
                        "unsupported lengthFieldLength: " + _lengthFieldLength + " (expected: 1, 2, 4, or 8)");
                
            }

            return frameLength;
        }

        protected NetworkData ExtractFrame(NetworkData data, int offset, int length)
        {
            try
            {
                var newData = new byte[length];
                Array.Copy(data.Buffer, offset, newData, 0, length);
                return NetworkData.Create(data.RemoteHost, newData, length);
            }
            catch (Exception ex)
            {
                throw new HeliosException(
                    string.Format("Error while copying {0} bytes from buffer of length {1} from starting index {2} to {3} into buffer of length {0}",
                    length, data.Length, offset, offset + length)
                    , ex);
            }
        }
    }
}