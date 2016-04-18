using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;

namespace Helios.Channels
{
    public interface IChannelConfiguration : IConnectionConfig
    {
        TimeSpan ConnectTimeout { get; set; }

        int MaxMessagesPerRead { get; set; }

        IByteBufAllocator Allocator { get; set; }

        IRecvByteBufAllocator RecvByteBufAllocator { get; set; }

        IMessageSizeEstimator MessageSizeEstimator { get; set; }
    }
}
