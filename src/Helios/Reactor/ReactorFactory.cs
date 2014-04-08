using Helios.Channels;
using Helios.Net;
using Helios.Reactor.Tcp;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// Factory used for configuring reactors that are consumed by <see cref="IChannel"/>s and others
    /// </summary>
    public static class ReactorFactory
    {
        public static IConnection ConfigureTcpReactor(INode localAddress,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
        {
            return new ReactorConnectionAdapter(new HighPerformanceTcpReactor(localAddress.Host, localAddress.Port));            
        }
    }
}
