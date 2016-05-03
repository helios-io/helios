using System;
using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// Interface used for spawning new <see cref="IConnection"/> objects
    /// </summary>
    [Obsolete()]
    public interface IConnectionFactory
    {
        IConnection NewConnection();

        IConnection NewConnection(INode remoteEndpoint);

        IConnection NewConnection(INode localEndpoint, INode remoteEndpoint);
    }
}