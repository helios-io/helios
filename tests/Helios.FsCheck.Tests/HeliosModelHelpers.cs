// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

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