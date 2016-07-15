// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Linq;

namespace Helios.Util.Collections
{
    /// <summary>
    ///     Static helpers for working with collections
    /// </summary>
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Subset<T>(this IEnumerable<T> obj, int offset, int count)
        {
            return Subset(obj.ToList(), offset, count);
        }

        public static IList<T> Subset<T>(this IList<T> obj, int offset, int count)
        {
            //Guard against bad input
            offset.NotLessThan(0);
            count.NotLessThan(0);
            (obj.Count - (offset + count)).NotNegative();

            var resultantList = new List<T>();

            for (var i = offset; i < offset + count; i++)
            {
                resultantList.Add(obj[i]);
            }

            return resultantList;
        }
    }
}