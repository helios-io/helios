using System;

namespace Helios.Core.Net.Providers
{
    [Flags]
    public enum ConnectionProviderType
    {
        Stateless = 0, //doesn't retain any connections - fire-and-forget
        Pooled = 1, //maintains a pool of connections
        Keyed = 2, //maintains a keyed list of connections
    };
}