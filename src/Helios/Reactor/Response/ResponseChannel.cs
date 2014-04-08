using Helios.Net;
using Helios.Ops;

namespace Helios.Reactor.Response
{
    /// <summary>
    /// Abstract base class used to wrap a socket generated from a <see cref="IReactor"/> into its own
    /// <see cref="IConnection"/> with its own <see cref="IEventLoop"/>.
    /// </summary>
    public abstract class ResponseChannel : IConnection
    {
    }
}
