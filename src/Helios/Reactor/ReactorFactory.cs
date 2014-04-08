using System;
using Helios.Channels;
using Helios.Net;
using Helios.Ops;
using Helios.Reactor.Tcp;
using Helios.Reactor.Udp;
using Helios.Topology;

namespace Helios.Reactor
{
    /// <summary>
    /// Factory used for configuring reactors that are consumed by <see cref="IChannel"/>s and others
    /// </summary>
    public static class ReactorFactory
    {
        public static ReactorBase ConfigureTcpReactor(INode localAddress, IExecutor internalExecutor = null, bool clientConnectionsAreProxies = true,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE, int workerThreads = 2)
        {
            if(clientConnectionsAreProxies)
                return new TcpProxyReactor(localAddress.Host, localAddress.Port, EventLoopFactory.CreateThreadedEventLoop(workerThreads, internalExecutor));
            throw new NotImplementedException();
        }

        public static ReactorBase ConfigureUdpReactor(INode localAddress, IExecutor internalExecutor = null,
            bool clientConnectionsAreProxies = true,
            int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE, int workerThreads = 2)
        {
            if (clientConnectionsAreProxies)
                return new ProxyUdpReactor(localAddress.Host, localAddress.Port, EventLoopFactory.CreateThreadedEventLoop(workerThreads, internalExecutor));
            throw new NotImplementedException();
        }
    }
}
