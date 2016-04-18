using Helios.Buffers;

namespace Helios.Channels
{
    /// <summary>
    /// A default <see cref="IMessageSizeEstimator"/>, designed for working with <see cref="IByteBuf"/> primarily.
    /// </summary>
    public sealed class DefaultMessageSizeEstimator : IMessageSizeEstimator
    {
        /// <summary>
        /// Use this value to signal unknown size.
        /// </summary>
        public const int UnknownSize = -1;

        sealed class DefaultHandle : IMessageSizeEstimatorHandle
        {
            private readonly int _unknownSize;

            public DefaultHandle(int unknownSize)
            {
                _unknownSize = unknownSize;
            }

            public int Size(object obj)
            {
                var byteBuf = obj as ByteBuffer;
                if (byteBuf != null)
                {
                    return byteBuf.ReadableBytes;
                }
                else if (obj is byte[])
                {
                    var bytes = obj as byte[];
                    return bytes.Length;
                }
                return _unknownSize;
            }
        }

        private readonly IMessageSizeEstimatorHandle _handle;

        public static readonly IMessageSizeEstimator Default = new DefaultMessageSizeEstimator(UnknownSize);

        private DefaultMessageSizeEstimator(int unknownSize)
        {
            _handle = new DefaultHandle(unknownSize);
        }

        public IMessageSizeEstimatorHandle NewHandle()
        {
            return _handle;
        }
    }
}