using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Helios.Buffers
{
    /// <summary>
    /// Utility class for working with direct <see cref="ByteBuffer"/> instances
    /// </summary>
    public static class ByteBufferUtil
    {

        private static readonly char[] HexChars = "0123456789ABCDEF".ToCharArray();

        public static string HexDump(IByteBuf buffer, int bytesPerLine = 16)
        {
            return HexDump(buffer.ToArray(), bytesPerLine);
        }

        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '.' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }


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
                return encoding.GetString(src.Array, src.ArrayOffset + readerIndex, len);
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
                    buffer.Release();
                }
            }
        }
    }
}
