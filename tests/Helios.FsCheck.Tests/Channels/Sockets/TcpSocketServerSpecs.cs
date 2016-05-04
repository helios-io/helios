// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using FsCheck;
using FsCheck.Experimental;
using Xunit;

namespace Helios.FsCheck.Tests.Channels.Sockets
{
    public class TcpSocketServerSpecs
    {
        [Fact(Skip = "Resolved XUnit issue, but need to disable shrinking")]
        public void TcpSeverSocketChannel_should_obey_model()
        {
            var model = new TcpServerSocketChannelStateMachine();
            try
            {
                model.ToProperty().VerboseCheckThrowOnFailure();
            }
            finally
            {
                model.ShutdownAll();
            }
        }
    }
}

