namespace Helios.Channels.Socket
{
    /// <summary>
    /// A TCP / IP socket <see cref="IChannel"/>
    /// </summary>
    public interface ISocketChannel : IChannel
    {
        new IServerSocketChannel Parent { get; }

        new ISocketChannelConfig Config { get; }

        /// <summary>
        /// Returns tue if and only if the remote peer shut down its output so that no more
        /// data is received from this cahnnel.
        /// </summary>
        bool InputShutdown { get; }

        bool OutputShutdown { get; }

        ChannelFuture ShutDownOutput();

        ChannelFuture ShutDownOutput(ChannelPromise<bool> future);
    }
}
