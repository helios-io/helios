using System;
using System.Collections.Generic;
using Helios.Buffers;

namespace Helios.FsCheck.Tests.Buffers
{
    public static class BufferHelpers
    {
        public static List<List<T>> ChunkOps<T>(List<T> source, int chunkSize)
        {
            var list = new List<List<T>>();
            for (var i = 0; i < source.Count; i = i + chunkSize)
            {
                list.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
            }
            return list;
        }

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