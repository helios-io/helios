using System;
using Helios.Core.Topology;

namespace Helios.Core.Net
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
