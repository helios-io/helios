using System.Collections.Generic;

namespace Helios.Core.Net
{
    /// <summary>
    /// Keyed connection provider, which allows connection look-ups and searches
    /// </summary>
    /// <typeparam name="TKey">The key used for lookups</typeparam>
    public interface IKeyedConnectionProvider<TKey> : IConnectionProvider
    {
        IConnection GetConnection(TKey key);

        bool HasConnection(TKey key);

        IEnumerable<TKey> Keys { get; }

        IEnumerable<KeyValuePair<TKey, IConnection>> Connections { get; }

        int Count { get; }
    }
}