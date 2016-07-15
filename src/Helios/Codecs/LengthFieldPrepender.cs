// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Helios.Buffers;
using Helios.Channels;

namespace Helios.Codecs
{
    public class LengthFieldPrepender : MessageToMessageEncoder<IByteBuf>
    {
        private readonly ByteOrder _byteOrder;
        private readonly int _lengthAdjustment;
        private readonly bool _lengthFieldIncludesLengthFieldLength;
        private readonly int _lengthFieldLength;

        /// <summary>
        ///     Creates a new <see cref="LengthFieldPrepender" /> instance.
        /// </summary>
        /// <param name="lengthFieldLength">
        ///     The length of the prepended length field.
        ///     Only 1, 2, 4, and 8 are allowed.
        /// </param>
        public LengthFieldPrepender(int lengthFieldLength)
            : this(lengthFieldLength, false)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="LengthFieldPrepender" /> instance.
        /// </summary>
        /// <param name="lengthFieldLength">
        ///     The length of the prepended length field.
        ///     Only 1, 2, 4, and 8 are allowed.
        /// </param>
        /// <param name="lengthFieldIncludesLengthFieldLength">
        ///     If <c>true</c>, the length of the prepended length field is added
        ///     to the value of the prepended length field.
        /// </param>
        public LengthFieldPrepender(int lengthFieldLength, bool lengthFieldIncludesLengthFieldLength)
            : this(lengthFieldLength, 0, lengthFieldIncludesLengthFieldLength)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="LengthFieldPrepender" /> instance.
        /// </summary>
        /// <param name="lengthFieldLength">
        ///     The length of the prepended length field.
        ///     Only 1, 2, 4, and 8 are allowed.
        /// </param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        public LengthFieldPrepender(int lengthFieldLength, int lengthAdjustment)
            : this(lengthFieldLength, lengthAdjustment, false)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="LengthFieldPrepender" /> instance.
        /// </summary>
        /// <param name="lengthFieldLength">
        ///     The length of the prepended length field.
        ///     Only 1, 2, 4, and 8 are allowed.
        /// </param>
        /// <param name="lengthFieldIncludesLengthFieldLength">
        ///     If <c>true</c>, the length of the prepended length field is added
        ///     to the value of the prepended length field.
        /// </param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        public LengthFieldPrepender(int lengthFieldLength, int lengthAdjustment,
            bool lengthFieldIncludesLengthFieldLength)
            : this(ByteOrder.LittleEndian, lengthFieldLength, lengthAdjustment, lengthFieldIncludesLengthFieldLength)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="LengthFieldPrepender" /> instance.
        /// </summary>
        /// <param name="byteOrder">The <see cref="ByteOrder" /> of the length field.</param>
        /// <param name="lengthFieldLength">
        ///     The length of the prepended length field.
        ///     Only 1, 2, 4, and 8 are allowed.
        /// </param>
        /// <param name="lengthFieldIncludesLengthFieldLength">
        ///     If <c>true</c>, the length of the prepended length field is added
        ///     to the value of the prepended length field.
        /// </param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        public LengthFieldPrepender(ByteOrder byteOrder, int lengthFieldLength, int lengthAdjustment,
            bool lengthFieldIncludesLengthFieldLength)
        {
            if (lengthFieldLength != 1 && lengthFieldLength != 2 &&
                lengthFieldLength != 4 && lengthFieldLength != 8)
            {
                throw new ArgumentException(
                    "lengthFieldLength must be either 1, 2, 3, 4, or 8: " +
                    lengthFieldLength, "lengthFieldLength");
            }

            _byteOrder = byteOrder;
            _lengthFieldLength = lengthFieldLength;
            _lengthFieldIncludesLengthFieldLength = lengthFieldIncludesLengthFieldLength;
            _lengthAdjustment = lengthAdjustment;
        }

        protected override void Encode(IChannelHandlerContext context, IByteBuf message, List<object> output)
        {
            var length = message.ReadableBytes + _lengthAdjustment;
            if (_lengthFieldIncludesLengthFieldLength)
            {
                length += _lengthFieldLength;
            }

            if (length < 0)
            {
                throw new ArgumentException("Adjusted frame length (" + length + ") is less than zero");
            }

            switch (_lengthFieldLength)
            {
                case 1:
                    if (length >= 256)
                    {
                        throw new ArgumentException("length of object does not fit into one byte: " + length);
                    }
                    output.Add(context.Allocator.Buffer(1).WithOrder(_byteOrder).WriteByte((byte) length));
                    break;
                case 2:
                    if (length >= 65536)
                    {
                        throw new ArgumentException("length of object does not fit into a short integer: " + length);
                    }
                    output.Add(context.Allocator.Buffer(2).WithOrder(_byteOrder).WriteShort((short) length));
                    break;
                case 4:
                    output.Add(context.Allocator.Buffer(4).WithOrder(_byteOrder).WriteInt(length));
                    break;
                case 8:
                    output.Add(context.Allocator.Buffer(8).WithOrder(_byteOrder).WriteLong(length));
                    break;
                default:
                    throw new Exception("Unknown length field length");
            }
            output.Add(message.Retain());
        }
    }
}