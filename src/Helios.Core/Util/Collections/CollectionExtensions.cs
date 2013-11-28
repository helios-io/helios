using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Helios.Core.Util.Collections
{
    /// <summary>
    /// Static helpers for working with collections
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
