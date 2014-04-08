using System.Net;
using System.Net.Sockets;
using Helios.Net;
using Helios.Ops;

namespace Helios.Reactor.Response
{
    /// <summary>
    /// Response channel receives all of its events directly from the <see cref="ReactorBase"/> and doesn't maintain any internal buffers,
    /// nor does it directly interact with its socket in any way
    /// </summary>
    public class ReactorProxyResponseChannel : ReactorResponseChannel
    {
        public ReactorProxyResponseChannel(ReactorBase reactor, Socket outboundSocket, IEventLoop eventLoop) : base(reactor, outboundSocket, eventLoop)
        {
        }

        public ReactorProxyResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint, IEventLoop eventLoop) : base(reactor, outboundSocket, endPoint, eventLoop)
        {
        }

        /// <summary>
        /// Method is called directly by the <see cref="ReactorBase"/> implementation to send data to this <see cref="IConnection"/>
        /// </summary>
        /// <param name="data"></param>
        internal void ReactorReceive(NetworkData data)
        {
            OnReceive(data);
        }

        protected override void BeginReceiveInternal()
        {
            
        }

        protected override void StopReceiveInternal()
        {
            
        }
    }
}