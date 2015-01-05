using System.Threading.Tasks;

namespace Helios.Channel
{
    /// <summary>
    /// Promise class used by <see cref="IChannel"/> and <see cref="IChannelHandler"/> instances to return
    /// waitable <see cref="Task{ChannelTaskResult}"/> handles.
    /// </summary>
    public class ChannelPromise : TaskCompletionSource<ChannelTaskResult>
    {
        
    }
}