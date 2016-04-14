using System;
using System.Collections.Generic;

namespace Helios.Util.Collections
{
    /// <summary>
    /// Static helper methods for working with dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> hash, TKey key)
        {
            return hash.ContainsKey(key) ? hash[key] : default(TValue);
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, object> hash,
            TKey key)
        {
            return hash.ContainsKey(key) && hash[key] is TValue ? (TValue)hash[key] : default(TValue);
        }
    }
}
