using System;
using Helios.Core.Topology;

namespace Helios.Core.Connections
{
    /// <summary>
    /// Interface used for building connections
    /// </summary>
    public interface IConnectionBuilder
    {
        IConnection BuildConnection(INode node);

        TimeSpan ConnectionTimeout { get; }

        TimeSpan ServerPollingInterval { get; }
    }
}
