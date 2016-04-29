namespace Helios.Channels.Sockets
{
    /// <summary>
    /// Marker interface for channels which use TCP / UDP sockets
    /// </summary>
    public interface ISocketChannel : IChannel
    {
        new IServerSocketChannel Parent { get; }
        new ISocketChannelConfiguration Configuration { get; }
    }
}
