// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.IO;
using Helios.Buffers;
using Helios.Net;
using Helios.Tracing;

namespace Helios.Serialization
{
    /// <summary>
    ///     Encoder that adds the length of the outgoing frame to the front of the object (which should, in theory, make it
    ///     easier to decode)
    /// </summary>
    public class LengthFieldPrepender : MessageEncoderBase
    {
        private readonly int _lengthAdjustment;
        private readonly int _lengthFieldLength;
        private readonly bool _lengthIncludesLenghtFieldLength;

        public LengthFieldPrepender(int lengthFieldLength, bool lengthIncludesLenghtFieldLength)
            : this(lengthFieldLength, lengthIncludesLenghtFieldLength, 0)
        {
        }

        public LengthFieldPrepender(int lengthFieldLength) : this(lengthFieldLength, false)
        {
        }

        public LengthFieldPrepender(int lengthFieldLength, bool lengthIncludesLenghtFieldLength, int lengthAdjustment)
        {
            _lengthFieldLength = lengthFieldLength;
            _lengthIncludesLenghtFieldLength = lengthIncludesLenghtFieldLength;
            _lengthAdjustment = lengthAdjustment;
        }

        #region Static methods

        /// <summary>
        ///     Returns a default <see cref="LengthFieldPrepender" /> that uses a 4-byte header to describe the length of a frame
        /// </summary>
        public static LengthFieldPrepender Default
        {
            get { return new LengthFieldPrepender(4, false); }
        }

        #endregion

        public void Encode(NetworkData data, out List<NetworkData> encoded)
        {
            var length = data.Length + _lengthAdjustment;
            if (_lengthIncludesLenghtFieldLength)
            {
                length += _lengthFieldLength;
            }

            encoded = new List<NetworkData>();
            if (length < 0)
                throw new ArgumentException(string.Format("Adjusted frame length ({0}) is less than zero", length));

            var newData = new byte[0];
            using (var memoryStream = new MemoryStream())
            {
                switch (_lengthFieldLength)
                {
                    case 1:
                        if (length >= 256)
                            throw new ArgumentException("length of object does not fit into one byte: " + length);
                        memoryStream.Write(BitConverter.GetBytes((byte) length), 0, 1);
                        break;
                    case 2:
                        if (length >= 65536)
                            throw new ArgumentException("length of object does not fit into a short integer: " + length);
                        memoryStream.Write(BitConverter.GetBytes((ushort) length), 0, 2);
                        break;
                    case 4:
                        memoryStream.Write(BitConverter.GetBytes((uint) length), 0, 4);
                        break;
                    case 8:
                        memoryStream.Write(BitConverter.GetBytes((long) length), 0, 8);
                        break;
                    default:
                        throw new Exception("Unknown lenght field length");
                }

                memoryStream.Write(data.Buffer, 0, data.Length);
                newData = memoryStream.GetBuffer();
            }

            var networkData = NetworkData.Create(data.RemoteHost, newData,
                _lengthIncludesLenghtFieldLength ? length : length + _lengthFieldLength);
            encoded.Add(networkData);
            HeliosTrace.Instance.EncodeSuccess();
        }

        public override void Encode(IConnection connection, IByteBuf buffer, out List<IByteBuf> encoded)
        {
            var length = buffer.ReadableBytes + _lengthAdjustment;
            if (_lengthIncludesLenghtFieldLength)
            {
                length += _lengthFieldLength;
            }

            encoded = new List<IByteBuf>();
            var sourceByteBuf = connection.Allocator.Buffer(_lengthFieldLength + length);
            if (length < 0)
                throw new ArgumentException(string.Format("Adjusted frame length ({0}) is less than zero", length));

            switch (_lengthFieldLength)
            {
                case 1:
                    if (length >= 256)
                        throw new ArgumentException("length of object does not fit into one byte: " + length);
                    sourceByteBuf.WriteByte(length);
                    break;
                case 2:
                    if (length >= 65536)
                        throw new ArgumentException("length of object does not fit into a short integer: " + length);
                    sourceByteBuf.WriteShort((ushort) length);
                    break;
                case 4:
                    unchecked
                    {
                        sourceByteBuf.WriteUnsignedInt((uint) length);
                    }
                    break;
                case 8:
                    sourceByteBuf.WriteLong(length);
                    break;
                default:
                    throw new Exception("Unknown length field length");
            }
            sourceByteBuf.WriteBytes(buffer);
            encoded.Add(sourceByteBuf);
            HeliosTrace.Instance.EncodeSuccess();
        }

        public override IMessageEncoder Clone()
        {
            return new LengthFieldPrepender(_lengthFieldLength, _lengthIncludesLenghtFieldLength, _lengthAdjustment);
        }
    }
}