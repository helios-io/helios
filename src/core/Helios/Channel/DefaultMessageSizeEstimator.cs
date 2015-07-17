using Helios.Buffers;
using System;

namespace Helios.Channel
{
    /// <summary>
    /// Default <see cref="IMessageSizeEstimator"/> implementation which supports the estimation of the size of
    /// <see cref="ByteBuffer"/>, <see cref="ByteBufferHolder"/> and <see cref="FileRegion"/>.
    /// </summary>
    public class DefaultMessageSizeEstimator : IMessageSizeEstimator
    {
        /// <summary>
        /// Return the default implementation which returns <code>-1</code> for unknown messages.
        /// </summary>
        public static readonly IMessageSizeEstimator Default = new DefaultMessageSizeEstimator(0);

        private IMessageSizeEstimatorHandle handle;

        /// <summary>
        /// Create new instance of DefaultMessageSizeEstimator
        /// </summary>
        /// <param name="unknownSize">Provide default message size for unknown message types.</param>
        public DefaultMessageSizeEstimator(int unknownSize)
        {
            if (unknownSize < 0)
                throw new ArgumentOutOfRangeException("unknownSize", "Parameter cannot be < 0. Expected value >= 0.");

            handle = new HandleImplementation(unknownSize);
        }

        public IMessageSizeEstimatorHandle NewHandle()
        {
            return handle;
        }

        private class HandleImplementation : IMessageSizeEstimatorHandle
        {
            private int _unknownSize;

            public HandleImplementation(int unknownSize)
            {
                this._unknownSize = unknownSize;
            }

            public int Size(object message)
            {
                if (message is IByteBuf)
                {
                    return ((IByteBuf)message).ReadableBytes;
                }
                // TODO ByteBufferHolder
                //if (message is ByteBufferHolder)
                //{
                //    return ((ByteBufferHolder)message).Content().ReadableBytes;
                //}
                // TODO FileRegion
                //if (message is FileRegion)
                //{
                //    return 0;
                //}

                return _unknownSize;
            }
        }
    }
}