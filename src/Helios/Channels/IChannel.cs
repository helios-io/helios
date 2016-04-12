using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Helios.Ops;

namespace Helios.Channels
{
    /// <summary>
    /// Represents the full set of operations that can be executed against an underlying Helios Transport.
    /// </summary>
    public interface IChannel
    {
        IEventLoop EventLoop { get; }

        IChannel Parent { get; }

        bool DisconnectSupported { get; }

        bool Open { get; }

        bool Active { get; }

        bool Registered { get; }

        EndPoint LocalAddress { get; }

        EndPoint RemoteAddress { get; }

        bool IsWritable { get; }

        IChannelPipeline Pipeline { get; }

        IConnectionConfig Configuration { get; }

        Task CloseCompletion { get; }

        Task DeregisterAsync();

        Task BindAsync(EndPoint localAddress);

        Task ConnectAsync(EndPoint remoteAddress);

        Task ConnectAsync(EndPoint remoteAddress, EndPoint localAddress);

        Task DisconnectAsync();

        Task CloseAsync();

        IChannel Read();

        Task WriteAsync(object message);

        IChannel Flush();

        Task WriteAndFlushAsync(object message);
    }
}
