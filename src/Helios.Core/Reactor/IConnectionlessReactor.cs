using System;

namespace Helios.Core.Reactor
{
    /// <summary>
    /// Interface for connectionless reactors
    /// </summary>
    public interface IConnectionlessReactor : IReactor
    {
        event EventHandler<ReactorReceivedDataEventArgs> DataAvailable;
    }
}