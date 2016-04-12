using System.Collections.Generic;

namespace Helios.Channels
{
    public interface IChannelPipeline : IEnumerable<IChannelHandler>
    {
        
    }
}