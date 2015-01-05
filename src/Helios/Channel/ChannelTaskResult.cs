using System.Threading.Tasks;

namespace Helios.Channel
{
    /// <summary>
    /// Class used in combination with <see cref="Task"/> and <see cref="TaskCompletionSource{T}"/> to expose
    /// a handle to the originating <see cref="IChannel"/> during asynchronous operations.
    /// </summary>
    public sealed class ChannelTaskResult
    {
        public ChannelTaskResult(IChannel channel)
        {
            Channel = channel;
        }

        public IChannel Channel { get; private set; }
    }
}
