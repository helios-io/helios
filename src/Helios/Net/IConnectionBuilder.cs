using System;
using Helios.Topology;

namespace Helios.Net
{
    /// <summary>
    /// Interface used for building connections
    /// </summary>
    public interface IConnectionBuilder
    {
        TimeSpan Timeout { get; }

        IConnection BuildConnection(INode node);
    }
}
