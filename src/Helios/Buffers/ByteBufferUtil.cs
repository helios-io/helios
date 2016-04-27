using System;
using System.Text;

namespace Helios.Buffers
{
    /// <summary>
    /// Utility class for working with direct <see cref="ByteBuffer"/> instances
    /// </summary>
    public static class ByteBufferUtil
    {
        /// <summary>
        /// Default initial capacity = 256 bytes
        /// </summary>
        public const int DEFAULT_INITIAL_CAPACITY = 256;

        /// <summary>
        ///  Toggles the endianness of the specified 64-bit long integer.
        /// </summary>
        public static long SwapLong(long value)
        {
            return (((long)SwapInt((int)value) & 0xFFFFFFFF) << 32)
                | ((long)SwapInt((int)(value >> 32)) & 0xFFFFFFFF);
        }

        /// <summary>
        /// Toggles the endianness of the specified 32-bit integer.
        /// </summary>
        public static int SwapInt(int value)
        {
            return ((SwapShort((short)value) & 0xFFFF) << 16)
                | (SwapShort((short)(value >> 16)) & 0xFFFF);
        }

        /// <summary>
        /// Toggles the endianness of the specified 16-bit integer.
        /// </summary>
        public static short SwapShort(short value)
        {
            return (short)(((value & 0xFF) << 8) | (value >> 8) & 0xFF);
        }

        public static string DecodeString(IByteBuf src, int readerIndex, int len, Encoding encoding)
        {
            if (len == 0)
            {
                return string.Empty;
            }

            if (src.HasArray)
            {
                return encoding.GetString(src.UnderlyingArray, src.ArrayOffset + readerIndex, len);
            }
            else
            {
                IByteBuf buffer = src.Allocator.Buffer(len);
                try
                {
                    buffer.WriteBytes(src, readerIndex, len);
                    return encoding.GetString(buffer.ToArray(), 0, len);
                }
                finally
                {
                    // Release the temporary buffer again.
                    // todo: reference counting
                }
            }
        }
    }
}
