using System.Threading.Tasks;

namespace Helios.Channels
{
    public class VoidChannelPromise : ChannelPromise<bool> {
        public VoidChannelPromise(IChannel channel) : base(channel, new TaskCompletionSource<bool>())
        {
        }
    }
}