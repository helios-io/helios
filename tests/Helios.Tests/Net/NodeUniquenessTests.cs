using System.Collections.Generic;
using System.Net;
using Helios.Topology;
using NUnit.Framework;

namespace Helios.Tests.Net
{
    [TestFixture(Description = "Tests that guarantee whether or not two INode are value-equivalent")]
    public class NodeUniquenessTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Test]
        public void Should_find_two_equivalent_nodes_as_equal()
        {
            var node1 = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337);
            var node2 = NodeBuilder.FromEndpoint(new IPEndPoint(IPAddress.Loopback, 1337));

            Assert.IsTrue(node1.Equals(node2));
        }

        [Test]
        public void Should_find_two_equivalent_INodes_in_dictionary_without_reference_equality()
        {
            var node1 = NodeBuilder.BuildNode().Host(IPAddress.Loopback).WithPort(1337);
            var node2 = NodeBuilder.FromEndpoint(new IPEndPoint(IPAddress.Loopback, 1337));

            var nodeDict = new Dictionary<INode, string>();
            nodeDict.Add(node1, "test!");

            Assert.AreEqual("test!", nodeDict[node2]);
        }

        

        #endregion
    }
}
