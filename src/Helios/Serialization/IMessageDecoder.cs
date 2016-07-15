// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using Helios.Buffers;
using Helios.Net;

namespace Helios.Serialization
{
    /// <summary>
    ///     Used to encode <see cref="NetworkData" /> inside Helios
    /// </summary>
    public interface IMessageDecoder
    {
        /// <summary>
        ///     Encodes <see cref="buffer" /> into a format that's acceptable for <see cref="IConnection" />.
        ///     Might return a list of decoded objects in <see cref="decoded" />, and it's up to the handler to determine
        ///     what to do with them.
        /// </summary>
        void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded);

        /// <summary>
        ///     Creates a deep clone of this <see cref="IMessageDecoder" /> instance with the exact same settings as the parent.
        /// </summary>
        /// <returns></returns>
        IMessageDecoder Clone();
    }

    public abstract class MessageDecoderBase : IMessageDecoder
    {
        public abstract void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded);
        public abstract IMessageDecoder Clone();
    }

    /// <summary>
    ///     Dummy decoder that doesn't actually do anything
    /// </summary>
    public class NoOpDecoder : MessageDecoderBase
    {
        public override void Decode(IConnection connection, IByteBuf buffer, out List<IByteBuf> decoded)
        {
            var outBuffer = connection.Allocator.Buffer(buffer.ReadableBytes);
            buffer.ReadBytes(outBuffer);
            decoded = new List<IByteBuf> {outBuffer};
        }

        public override IMessageDecoder Clone()
        {
            return new NoOpDecoder();
        }
    }
}