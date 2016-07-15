// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;

namespace Helios.Util.Collections
{
    /// <summary>
    ///     Static helper methods for working with dictionaries
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
            return hash.ContainsKey(key) && hash[key] is TValue ? (TValue) hash[key] : default(TValue);
        }
    }
}