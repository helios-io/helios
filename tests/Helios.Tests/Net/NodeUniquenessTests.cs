using System.Collections.Generic;
using System.Net;
using Helios.Topology;
using Xunit;

namespace Helios.Tests.Net
{
    public class NodeUniquenessTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Fact]
        public void Should_find_two_equivalent_nodes_as_equal()
        {
            var node1 = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337);
            var node2 = NodeBuilder.FromEndpoint(new IPEndPoint(IPAddress.Loopback, 1337));

            Assert.True(node1.Equals(node2));
        }

        [Fact]
        public void Should_find_two_equivalent_INodes_in_dictionary_without_reference_equality()
        {
            var node1 = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337);
            var node2 = NodeBuilder.FromEndpoint(new IPEndPoint(IPAddress.Loopback, 1337));

            var nodeDict = new Dictionary<INode, string>();
            nodeDict.Add(node1, "test!");

            Assert.Equal("test!", nodeDict[node2]);
        }

        

        #endregion
    }
}
