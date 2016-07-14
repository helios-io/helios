// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Net;
using Helios.Net;
using Xunit;

namespace Helios.Tests.Net
{
    public class MulticastHelperTests
    {
        #region Setup / Teardown

        #endregion

        #region Tests

        [Fact]
        public void Should_mark_valid_IPv4_multicast_address_as_valid()
        {
            //arrange
            var validMulticastIp = "224.1.1.1";

            //act
            var isValid = MulticastHelper.IsValidMulticastAddress(IPAddress.Parse(validMulticastIp));

            //assert
            Assert.True(isValid);
        }

        [Fact]
        public void Should_mark_invalid_IPv4_multicast_address_as_invalid()
        {
            //arrange
            var invalidMulticastIp = "255.1.1.1";

            //act
            var isValid = MulticastHelper.IsValidMulticastAddress(IPAddress.Parse(invalidMulticastIp));

            //assert
            Assert.False(isValid);
        }

        #endregion
    }
}