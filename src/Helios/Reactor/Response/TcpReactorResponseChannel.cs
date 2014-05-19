using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Net;

namespace Helios.Reactor.Response
{
    /// <summary>
    /// A <see cref="ReactorResponseChannel"/> instance which manages all of the socket I/O for the child connection directly.
    /// 
    /// Shares the same underlying <see cref="IFiber"/> as the parent <see cref="IReactor"/> responsible for creating this child.
    /// </summary>
    public class TcpReactorResponseChannel : ReactorResponseChannel
    {
        /// <summary>
        /// shared buffer used by all incoming connections
        /// </summary>
        protected byte[] Buffer;

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, NetworkEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : this(reactor, outboundSocket, (IPEndPoint)outboundSocket.RemoteEndPoint, eventLoop, bufferSize)
        {
        }

        public TcpReactorResponseChannel(ReactorBase reactor, Socket outboundSocket, IPEndPoint endPoint, NetworkEventLoop eventLoop, int bufferSize = NetworkConstants.DEFAULT_BUFFER_SIZE)
            : base(reactor, outboundSocket, endPoint, eventLoop)
        {
            Buffer = new byte[bufferSize];
        }

        public override void Configure(IConnectionConfig config)
        {
            throw new NotImplementedException();
        }

        protected override void BeginReceiveInternal()
        {
            //Socket.BeginReceive()
        }

        protected override void StopReceiveInternal()
        {
            throw new NotImplementedException();
        }

        public override void Send(NetworkData payload)
        {
            base.Send(payload);
        }

        public override Task SendAsync(NetworkData payload)
        {
            return base.SendAsync(payload);
        }
    }
}