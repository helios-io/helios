using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    /// Task implementation which contains a reference back to the original <see cref="IChannel"/> responsible
    /// for executing it.
    /// </summary>
    public class ChannelFuture
    {
        public ChannelFuture(IChannel channel, Task task)
        {
            Task = task;
            Channel = channel;
        }

        public IChannel Channel { get; private set; }

        public Task Task { get; private set; }

        public bool IsCanceled { get { return Task.IsCanceled; } }
        public bool IsFaulted { get { return Task.IsFaulted; } }
        public bool IsCompleted { get { return Task.IsCompleted; } }
    }

    /// <summary>
    /// Task implementation which contains a reference back to the original <see cref="IChannel"/> responsible
    /// for executing it.
    /// </summary>
    public class ChannelFuture<T> : ChannelFuture
    {
        public ChannelFuture(IChannel channel, Task<T> task) : base(channel, task)
        {
            Task = task;
        }

        public new Task<T> Task { get; private set; }
       
    }
}
