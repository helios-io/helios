using System;
using System.Net;
using System.Net.Sockets;
using Helios.Topology;

namespace Helios.Net
{
    /// <summary>
    /// Data arrived via a remote host - used to help provide a common interface
    /// on our IConnection members
    /// </summary>
    public class NetworkData
    {
        public INode RemoteHost { get; protected set; }

        public DateTime Recieved { get; protected set; }

        public byte[] Data { get; protected set; }

        public int Bytes { get; protected set; }

        public NetworkData()
        {
            Recieved = DateTime.UtcNow;
        }

        public static NetworkData Create(INode node, byte[] data, int bytes)
        {
            return new NetworkData()
            {
                Data = data,
                Bytes = bytes,
                RemoteHost = node
            };
        }

        public static NetworkData Create(UdpReceiveResult receiveResult)
        {
            return new NetworkData()
            {
                Data = receiveResult.Buffer,
                Bytes = receiveResult.Buffer.Length,
                RemoteHost = receiveResult.RemoteEndPoint.ToNode(TransportType.Udp)
            };
        }
    }
}
