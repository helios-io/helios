using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    /// Allocates buffers for socket read and receive operations, with enough capacity to read all received
    /// data in one shot.
    /// </summary>
    public interface IRecvByteBufAllocator
    {
        /// <summary>
        /// Creates a new handle for estimating incoming receive buffer sizes and creating buffers
        /// based on those estimates.
        /// </summary>
        IRecvByteBufferAllocatorHandle NewHandle();
    }
}
