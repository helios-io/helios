// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Helios.Util;

namespace Helios.Channels.Sockets
{
    public class SocketChannelAsyncOperation : SocketAsyncEventArgs
    {
        public SocketChannelAsyncOperation(AbstractSocketChannel channel)
            : this(channel, true)
        {
        }

        public SocketChannelAsyncOperation(AbstractSocketChannel channel, bool setEmptyBuffer)
        {
            Contract.Requires(channel != null);

            this.Channel = channel;
            this.Completed += AbstractSocketChannel.IoCompletedCallback;
            if (setEmptyBuffer)
            {
                this.SetBuffer(ByteArrayExtensions.Empty, 0, 0);
            }
        }

        public void Validate()
        {
            SocketError socketError = this.SocketError;
            if (socketError != SocketError.Success)
            {
                throw new SocketException((int) socketError);
            }
        }

        public AbstractSocketChannel Channel { get; private set; }
    }
}