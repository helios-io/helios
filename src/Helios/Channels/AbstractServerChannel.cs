using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Concurrency;
using Helios.Util.Concurrency;

namespace Helios.Channels
{
    /// <summary>
    /// A skeleton of a server-side <see cref="IChannel"/> implementation, which
    /// does not allow any of the following operations:
    /// 
    /// * <see cref="IChannel.ConnectAsync(EndPoint)"/>
    /// * <see cref="IChannel.DisconnectAsync()"/>
    /// * <see cref="IChannel.WriteAsync(object)"/>
    /// * <see cref="IChannel.Flush()"/>
    /// </summary>
    public abstract class AbstractServerChannel : AbstractChannel, IServerChannel
    {
        protected AbstractServerChannel() : base(null)
        {
        }

        protected override EndPoint RemoteAddressInternal { get { return null; } }

        protected override void DoDisconnect()
        {
            throw new NotSupportedException();
        }

        protected override void DoWrite(ChannelOutboundBuffer input)
        {
            throw new NotSupportedException();
        }

        protected override object FilterOutboundMessage(object msg)
        {
            throw new NotSupportedException();
        }

        protected override IChannelUnsafe NewUnsafe()
        {
            return new DefaultServerUnsafe(this);
        }

        sealed class DefaultServerUnsafe : AbstractUnsafe
        {
            public DefaultServerUnsafe(AbstractChannel channel) : base(channel)
            {
            }

            public override Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress)
            {
                return TaskEx.FromException(new NotSupportedException());
            }
        }
    }
}
