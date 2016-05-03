using System.Collections.Generic;
using System.Net;
using Helios.Channels;

namespace Helios.FsCheck.Tests.Channels.Sockets.Models
{
    public interface ITcpServerSocketModel
    {
        IPEndPoint BoundAddress { get; }
        IChannel Self { get; }
        IReadOnlyList<IChannel> LocalChannels { get; }
        IReadOnlyList<IPEndPoint> RemoteClients { get; }
        IReadOnlyList<int> LastReceivedMessages { get; }

        IReadOnlyList<int> WrittenMessages { get; }

        ITcpServerSocketModel SetSelf(IChannel self);
        ITcpServerSocketModel SetOwnAddress(IPEndPoint endpoint);
        ITcpServerSocketModel AddLocalChannel(IChannel channel);
        ITcpServerSocketModel RemoveLocalChannel(IChannel channel);
        ITcpServerSocketModel AddClient(IPEndPoint endpoint);
        ITcpServerSocketModel RemoveClient(IPEndPoint endpoint);
        ITcpServerSocketModel ClearMessages();
        ITcpServerSocketModel WriteMessages(params int[] messages);
        ITcpServerSocketModel ReceiveMessages(params int[] messages);
    }
}
