using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels.Sockets
{
    public interface IServerSocketChannelConfiguration : IChannelConfiguration
    {
        int Backlog { get; set; }
    }
}
