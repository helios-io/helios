using System.Net;
using Helios.Core.Net;
using NUnit.Framework;

namespace Helios.Tests.Core.Net
{
    [TestFixture]
    public class MulticastHelperTests
    {
        #region Setup / Teardown
        #endregion

        #region Tests

        [Test]
        public void Should_mark_valid_IPv4_multicast_address_as_valid()
        {
            //arrange
            var validMulticastIp = "224.1.1.1";

            //act
            var isValid = MulticastHelper.IsValidMulticastAddress(IPAddress.Parse(validMulticastIp));

            //assert
            Assert.IsTrue(isValid);
        }

        [Test]
        public void Should_mark_invalid_IPv4_multicast_address_as_invalid()
        {
            //arrange
            var invalidMulticastIp = "255.1.1.1";

            //act
            var isValid = MulticastHelper.IsValidMulticastAddress(IPAddress.Parse(invalidMulticastIp));

            //assert
            Assert.IsFalse(isValid);
        }

        #endregion
    }
}
