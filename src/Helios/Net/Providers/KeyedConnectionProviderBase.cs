using System.Collections.Generic;
using System.Linq;

namespace Helios.Net.Providers
{
    public abstract class KeyedConnectionProviderBase<TKey> : ConnectionProviderBase, IKeyedConnectionProvider<TKey>
    {
// ReSharper disable once InconsistentNaming
        protected IDictionary<TKey, IConnection> _connections;

        protected KeyedConnectionProviderBase(IClusterManager clusterManager, IConnectionBuilder connectionBuilder) 
            : base(clusterManager, connectionBuilder, ConnectionProviderType.Keyed | ConnectionProviderType.Pooled)
        {
            _connections = new Dictionary<TKey, IConnection>();
        }

        public void AddConnection(TKey key, IConnection connection)
        {
            _connections[key] = connection;
        }

        public IConnection GetConnection(TKey key)
        {
            return _connections[key];
        }

        public bool HasConnection(TKey key)
        {
            return _connections.ContainsKey(key);
        }

        public IEnumerable<TKey> Keys { get { return _connections.Keys; } }

        public IEnumerable<KeyValuePair<TKey, IConnection>> Connections { get { return _connections.AsEnumerable(); } }
        public int Count { get { return _connections.Count; } }
    }
}