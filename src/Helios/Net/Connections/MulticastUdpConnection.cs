using System;
using System.Net.Sockets;
using Helios.Topology;

namespace Helios.Net.Connections
{
    /// <summary>
    /// Multi-cast implementation of a UDP 
    /// </summary>
    public class MulticastUdpConnection : UdpConnection
    {
        public MulticastUdpConnection(INode binding, INode multicastAddress, TimeSpan timeout) : base(binding, timeout)
        {
            MulticastAddress = multicastAddress;
            InitMulticastClient();
        }

        public MulticastUdpConnection(INode binding, INode multicastAddress) : this(binding, multicastAddress, NetworkConstants.DefaultConnectivityTimeout)
        {
        }

        public MulticastUdpConnection(UdpClient client) : base(client)
        {
            InitMulticastClient();
        }

        public INode MulticastAddress { get; protected set; }

        protected void InitMulticastClient()
        {
            if(Client == null)
                InitClient();
// ReSharper disable once PossibleNullReferenceException
            Client.JoinMulticastGroup(MulticastAddress.Host);
        }
    }
}