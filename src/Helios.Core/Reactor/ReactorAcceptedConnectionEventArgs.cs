using System;
using System.Net.Sockets;
using Helios.Core.Net;
using Helios.Core.Net.Connections;

namespace Helios.Core.Reactor
{
    public class ReactorAcceptedConnectionEventArgs : EventArgs
    {
        public StreamedConnectionBase Connection { get; protected set; }

        public static ReactorAcceptedConnectionEventArgs Create(TcpClient client)
        {
            return new ReactorAcceptedConnectionEventArgs() {Connection = new TcpConnection(client)};
        }
    }
}