using System;
using Helios.Core.Topology;

namespace Helios.Core.Exceptions
{
    public class HeliosNodeException : Exception
    {
        public HeliosNodeException(string message) : base(message) { }

        public HeliosNodeException(string message, Exception innerException) : base(message, innerException) { }

        public HeliosNodeException(string message, INode affectedNode) : base(message)
        {
            AffectedNode = affectedNode;
        }

        public HeliosNodeException(string message, Exception innerException, INode affectedNode)
            : base(message, innerException)
        {
            AffectedNode = affectedNode;
        }

        public HeliosNodeException() : base() { }

        public INode AffectedNode { get; protected set; }
    }
}
