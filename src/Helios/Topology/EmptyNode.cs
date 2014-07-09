using System.Net;

namespace Helios.Topology
{
    /// <summary>
    /// Special case pattern - uses an Empty node to denote when an item is local, rather than networked.
    /// </summary>
    public class EmptyNode : INode
    {
        public IPAddress Host { get; set; }
        public int Port { get; set; }
        public TransportType TransportType { get; set; }
        public string MachineName { get; set; }
        public string OS { get; set; }
        public string ServiceVersion { get; set; }
        public string CustomData { get; set; }

        private IPEndPoint _endPoint;
        public IPEndPoint ToEndPoint()
        {
            return _endPoint ?? (_endPoint =new IPEndPoint(IPAddress.None, 0));
        }

        public object Clone()
        {
            return Node.Empty();
        }
    }
}