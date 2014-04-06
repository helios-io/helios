namespace Helios.Channels.Socket
{
    /// <summary>
    /// A TPC/IP <see cref="IServerChannel"/> which accepts incoming TCP/IP connections
    /// </summary>
    public interface IServerSocketChannel : IServerChannel
    {
        new IServerSocketChannelConfig Config { get; }
    }
}