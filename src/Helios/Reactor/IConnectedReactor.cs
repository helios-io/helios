using System;

namespace Helios.Reactor
{
    /// <summary>
    /// Used for reactors that accept connections (such as TCP)
    /// </summary>
    public interface IConnectedReactor : IReactor
    {
        /// <summary>
        /// Event that is fired each time an incoming connection is received
        /// </summary>
        event EventHandler<ReactorAcceptedConnectionEventArgs> AcceptConnection;
    }
}