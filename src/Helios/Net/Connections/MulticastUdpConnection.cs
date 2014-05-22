using System;
using System.Net.Sockets;
using Helios.Buffers;
using Helios.Serialization;
using Helios.Topology;

namespace Helios.Net.Connections
{
    /// <summary>
    /// Multi-cast implementation of a UDP 
    /// </summary>
    public class MulticastUdpConnection : UdpConnection
    {
        public MulticastUdpConnection(NetworkEventLoop eventLoop, INode binding, INode multicastAddress, TimeSpan timeout, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : base(eventLoop, binding, timeout, encoder, decoder, allocator)
        {
            MulticastAddress = multicastAddress;
            InitMulticastClient();
        }

        public MulticastUdpConnection(NetworkEventLoop eventLoop, INode binding, INode multicastAddress, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : this(eventLoop, binding, multicastAddress, NetworkConstants.DefaultConnectivityTimeout, encoder, decoder, allocator)
        {
        }

        public MulticastUdpConnection(UdpClient client, IMessageEncoder encoder, IMessageDecoder decoder, IByteBufAllocator allocator)
            : base(client)
        {
            InitMulticastClient();
            Encoder = encoder;
            Decoder = decoder;
            Allocator = allocator;
        }

        public MulticastUdpConnection(UdpClient client) : this(client, Encoders.DefaultEncoder, Encoders.DefaultDecoder, UnpooledByteBufAllocator.Default)
        {
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