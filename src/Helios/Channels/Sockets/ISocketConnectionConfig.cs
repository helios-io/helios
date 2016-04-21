namespace Helios.Channels.Sockets
{
    /// <summary>
    ///     <see cref="IConnectionConfig" /> specific to sockets.
    /// </summary>
    public interface ISocketConnectionConfig : IConnectionConfig
    {
        int Linger { get; set; }

        int SendBufferSize { get; set; }

        int ReceiveBufferSize { get; set; }

        bool ReuseAddress { get; set; }

        bool KeepAlive { get; set; }

        bool NoDelay { get; set; }
    }

    public class DefaultSocketConnectionConfig : DefaultConnectionConfig, ISocketConnectionConfig
    {
        public int Linger { get; set; }
        public int SendBufferSize { get; set; }
        public int ReceiveBufferSize { get; set; }
        public bool ReuseAddress { get; set; }
        public bool KeepAlive { get; set; }
        public bool NoDelay { get; set; }
    }
}