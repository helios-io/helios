using Helios.Ops;

namespace Helios.Channels
{
    /// <summary>
    /// A <see cref="IChannel"/> that accepts an incoming connection attempt and creates its child <see cref="IChannel"/>s
    /// by accepting them.
    /// </summary>
    public interface IServerChannel : IChannel
    {
        IEventLoop ChildEventLoop();
    }
}
