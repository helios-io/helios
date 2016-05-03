using System;
using System.Collections.Generic;
using Helios.Buffers;

namespace Helios.FsCheck.Tests.Buffers
{
    public static class BufferHelpers
    {
        public static string PrintByteArray(byte[] bytes)
        {
            return "byte[" + string.Join("|", bytes) + "]";
        }

        public static object PrintByteBufferItem(object item)
        {
            if (item is byte[])
            {
                var bytes = item as byte[];
                return PrintByteArray(bytes);
            }

            return item;
        }
    }
}