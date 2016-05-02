using System;
using System.Collections.Generic;

namespace Helios.FsCheck.Tests
{
    public static class HeliosModelHelpers
    {
        public static List<List<T>> Chunk<T>(this List<T> source, int chunkSize)
        {
            var list = new List<List<T>>();
            for (var i = 0; i < source.Count; i = i + chunkSize)
            {
                list.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
            }
            return list;
        }
    }
}