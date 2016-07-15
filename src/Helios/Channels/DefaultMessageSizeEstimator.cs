// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Buffers;

namespace Helios.Channels
{
    /// <summary>
    ///     A default <see cref="IMessageSizeEstimator" />, designed for working with <see cref="IByteBuf" /> primarily.
    /// </summary>
    public sealed class DefaultMessageSizeEstimator : IMessageSizeEstimator
    {
        /// <summary>
        ///     Use this value to signal unknown size.
        /// </summary>
        public const int UnknownSize = -1;

        public static readonly IMessageSizeEstimator Default = new DefaultMessageSizeEstimator(UnknownSize);

        private readonly IMessageSizeEstimatorHandle _handle;

        private DefaultMessageSizeEstimator(int unknownSize)
        {
            _handle = new DefaultHandle(unknownSize);
        }

        public IMessageSizeEstimatorHandle NewHandle()
        {
            return _handle;
        }

        private sealed class DefaultHandle : IMessageSizeEstimatorHandle
        {
            private readonly int _unknownSize;

            public DefaultHandle(int unknownSize)
            {
                _unknownSize = unknownSize;
            }

            public int Size(object obj)
            {
                var byteBuf = obj as IByteBuf;
                if (byteBuf != null)
                {
                    return byteBuf.ReadableBytes;
                }
                if (obj is byte[])
                {
                    var bytes = obj as byte[];
                    return bytes.Length;
                }
                return _unknownSize;
            }
        }
    }
}