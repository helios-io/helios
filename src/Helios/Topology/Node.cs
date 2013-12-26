using System;
using System.Net;
using Helios.Net;
using Helios.Util;

namespace Helios.Topology
{
    /// <summary>
    /// Node belonging to a service
    /// </summary>
    public class Node : INode
    {
        public Node()
        {
            TransportType = TransportType.Tcp;
        }

        /// <summary>
        /// The IP address of this seed
        /// </summary>
        public IPAddress Host { get; set; }

        /// <summary>
        /// The name of this machine
        /// </summary>
        public string MachineName { get; set; }

        public string OS { get; set; }
        public string ServiceVersion { get; set; }

        /// <summary>
        /// A JSON blob representing arbtirary data about this node
        /// </summary>
        public string CustomData { get; set; }

        /// <summary>
        /// The port number of this node
        /// </summary>
        public int Port { get; set; }


        public TransportType TransportType { get; set; }

        /// <summary>
        /// A DateTime.Ticks representation of when we last heard from this node
        /// </summary>
        public long LastPulse { get; set; }

        public override int GetHashCode()
        {
            var hashCode = Host.GetHashCode();
            return hashCode;
        }

        public object Clone()
        {
            return new Node()
            {
                CustomData = CustomData.NotNull(s => (string) s.Clone()),
                Host = new IPAddress(Host.GetAddressBytes()),
                MachineName = MachineName.NotNull(s => (string) s.Clone()),
                Port = Port,
                TransportType = TransportType
            };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Host, Port);
        }

        #region Static methods

        public static INode Loopback(int port = NetworkConstants.InMemoryPort)
        {
            return NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(port);
        }

        public static INode Empty()
        {
            return new EmptyNode();
        }


        public static INode FromString(string nodeUri)
        {
            var uri = new Uri(nodeUri);
            return uri.ToNode();
        }


        #endregion
    }
}
