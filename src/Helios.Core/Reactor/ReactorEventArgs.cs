using System;
using System.Net.Sockets;
using Helios.Core.Net;
using Helios.Core.Net.Connections;

namespace Helios.Core.Reactor
{
    public class ReactorEventArgs : EventArgs
    {
        public IConnection Connection { get; protected set; }

        public static ReactorEventArgs Create(TcpClient client)
        {
            return new ReactorEventArgs() {Connection = new TcpConnection(client)};
        }
    }
}