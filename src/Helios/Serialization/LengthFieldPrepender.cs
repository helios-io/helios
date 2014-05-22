using System;
using System.Collections.Generic;
using System.IO;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// Encoder that adds the length of the outgoing frame to the front of the object (which should, in theory, make it easier to decode)
    /// </summary>
    public class LengthFieldPrepender : MessageEncoderBase
    {
        private readonly int _lengthFieldLength;
        private readonly bool _lengthIncludesLenghtFieldLength;
        private readonly int _lengthAdjustment;

        public LengthFieldPrepender(int lengthFieldLength, bool lengthIncludesLenghtFieldLength)
            : this(lengthFieldLength, lengthIncludesLenghtFieldLength, 0)
        {
        }

        public LengthFieldPrepender(int lengthFieldLength) : this(lengthFieldLength, false) { }

        public LengthFieldPrepender(int lengthFieldLength, bool lengthIncludesLenghtFieldLength, int lengthAdjustment)
        {
            _lengthFieldLength = lengthFieldLength;
            _lengthIncludesLenghtFieldLength = lengthIncludesLenghtFieldLength;
            _lengthAdjustment = lengthAdjustment;
        }

        public override void Encode(NetworkData data, out List<NetworkData> encoded)
        {
            var length = data.Length + _lengthAdjustment;
            if (_lengthIncludesLenghtFieldLength)
            {
                length += _lengthFieldLength;
            }

            encoded = new List<NetworkData>();
            if (length < 0) throw new ArgumentException(string.Format("Adjusted frame length ({0}) is less than zero", length));

            var newData = new byte[0];
            using (var memoryStream = new MemoryStream())
            {
                switch (_lengthFieldLength)
                {

                    case 1:
                        if (length >= 256) throw new ArgumentException("length of object does not fit into one byte: " + length);
                        memoryStream.Write(BitConverter.GetBytes((byte)length), 0, 1);
                        break;
                    case 2:
                        if (length >= 65536) throw new ArgumentException("length of object does not fit into a short integer: " + length);
                        memoryStream.Write(BitConverter.GetBytes((ushort)length), 0, 2);
                        break;
                    case 4:
                        memoryStream.Write(BitConverter.GetBytes((uint)length), 0, 4);
                        break;
                    case 8:
                        memoryStream.Write(BitConverter.GetBytes((long)length), 0, 8);
                        break;
                    default:
                        throw new Exception("Unknown lenght field length");
                }

                memoryStream.Write(data.Buffer, 0, data.Length);
                newData = memoryStream.GetBuffer();
            }

            var networkData = NetworkData.Create(data.RemoteHost, newData, _lengthIncludesLenghtFieldLength ? length : length + _lengthFieldLength);
            encoded.Add(networkData);
        }
    }
}