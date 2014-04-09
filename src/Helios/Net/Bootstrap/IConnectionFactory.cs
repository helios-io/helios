using Helios.Topology;

namespace Helios.Net.Bootstrap
{
    /// <summary>
    /// Interface used for spawning new <see cref="IConnection"/> objects
    /// </summary>
    public interface IConnectionFactory
    {
        IConnection NewConnection(INode localEndpoint, INode remoteEndpoint);
    }
}