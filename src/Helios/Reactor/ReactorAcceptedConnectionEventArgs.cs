using System;
using System.Net.Sockets;
using Helios.Net.Connections;

namespace Helios.Reactor
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