using System;
using System.Collections.Generic;

namespace Helios.Util.Collections
{
    /// <summary>
    /// Static helper methods for working with dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Dictionary helper that allows for idempotent updates. You don't need to care whether or not
        /// this item is already in the collection in order to update it.
        /// </summary>
        public static void AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> hash, TKey key, TValue value)
        {
            if (hash.ContainsKey(key))
                hash[key] = value;
            else
                hash.Add(key, value);
        }

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
