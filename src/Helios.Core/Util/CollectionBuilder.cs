using System.Collections.Generic;
using System.Linq;

namespace Helios.Core.Util
{
    /// <summary>
    /// Utility class for synthesizing arrays
    /// </summary>
    public static class CollectionBuilder
    {
        public static IEnumerable<TOut> Of<TOut>(this int i, TOut initialValue)
        {
            i.NotLessThan(0);

            var output = new List<TOut>(i);
            for (var n = 0; n < i; n++)
            {
                output.Add(initialValue);
            }
            return output;
        }
    }
}
