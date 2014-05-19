using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Channels;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    /// Used to encode <see cref="NetworkData"/> inside Helios
    /// </summary>
    public interface IMessageEncoder
    {
        /// <summary>
        /// Write data to the <see cref="IConnection"/>.
        /// 
        /// <see cref="data"/> will be encoded by the <see cref="Encode"/> method before being written.
        /// </summary>
        void Write(IConnection connection, NetworkData data);

        /// <summary>
        /// Encodes <see cref="data"/> into a format that's acceptable for <see cref="IConnection"/>.
        /// 
        /// Might return a list of encoded objects in <see cref="encoded"/>, and it's up to the handler to determine
        /// what to do with them.
        /// </summary>
        void Encode(IConnection connection, NetworkData data, out List<NetworkData> encoded);
    }

    public abstract class MessageEncoderBase : IMessageEncoder
    {
        public void Write(IConnection connection, NetworkData data)
        {
            List<NetworkData> messages;
            Encode(connection, data, out messages);
            foreach (var message in messages)
            {
                connection.Send(message);
            }
        }

        public abstract void Encode(IConnection connection, NetworkData data, out List<NetworkData> encoded);
    }

    /// <summary>
    /// Encoder that adds the length of the outgoing frame to the front of the object (which should, in theory, make it easier to decode)
    /// </summary>
    public class LengthFieldPrepender : MessageEncoderBase
    {
        private readonly int _lengthFieldLength;
        private readonly bool _lengthIncludesLenghtFieldLength;
        private readonly int _lengthAdjustment;

        public LengthFieldPrepender(int lengthFieldLength, bool lengthIncludesLenghtFieldLength)
            : this(lengthFieldLength, lengthIncludesLenghtFieldLength, lengthFieldLength)
        {
        }

        public LengthFieldPrepender(int lengthFieldLength) : this(lengthFieldLength, false) { }

        public LengthFieldPrepender(int lengthFieldLength, bool lengthIncludesLenghtFieldLength, int lengthAdjustment)
        {
            _lengthFieldLength = lengthFieldLength;
            _lengthIncludesLenghtFieldLength = lengthIncludesLenghtFieldLength;
            _lengthAdjustment = lengthAdjustment;
        }

        public override void Encode(IConnection connection, NetworkData data, out List<NetworkData> encoded)
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
                        memoryStream.Write(BitConverter.GetBytes((short)length), 0, 2);
                        break;
                    case 3:
                        if (length >= 16777216) throw new ArgumentException("length of object does not fit into a medium integer: " + length);
                        memoryStream.Write(BitConverter.GetBytes((int)length), 0, 3);
                        break;
                    case 4:
                        memoryStream.Write(BitConverter.GetBytes((int)length), 0, 4);
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

            var networkData = NetworkData.Create(data.RemoteHost, newData, length);
            encoded.Add(networkData);
        }
    }
}
