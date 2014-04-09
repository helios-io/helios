using Helios.Net.Bootstrap;

namespace Helios.Reactor.Bootstrap
{
    /// <summary>
    /// Factory interface for creating new <see cref="IReactor"/> instances
    /// </summary>
    public interface IServerFactory : IConnectionFactory
    {
        IReactor NewReactor();
    }
}