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
                throw new SocketException((int)socketError);
            }
        }

        public AbstractSocketChannel Channel { get; private set; }
    }
}
