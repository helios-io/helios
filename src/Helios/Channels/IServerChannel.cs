using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    /// Marker interface for <see cref="IChannel"/> implementations which act as inbound
    /// receivers for connections from external clients.
    /// </summary>
    public interface IServerChannel : IChannel
    {
    }
}
