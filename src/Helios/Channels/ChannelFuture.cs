using System;
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
        public bool IsCanceled { get { return Task.IsCanceled; } }
        public bool IsFaulted { get { return Task.IsFaulted; } }
        public bool IsCompleted { get { return Task.IsCompleted; } }
    }

    /// <summary>
    /// A <see cref="TaskCompletionSource{T}"/> implementation which propagates the original <see cref="IChannel"/> responsible for the
    /// request back to the called, by way of a <see cref="ChannelFuture{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The return type of the <see cref="ChannelFuture{T}"/></typeparam>
    public class ChannelPromise<T>
    {
        public ChannelPromise(IChannel channel) : this(channel, new TaskCompletionSource<T>()) { }

        public ChannelPromise(IChannel channel, TaskCompletionSource<T> completionSource)
        {
            CompletionSource = completionSource;
            Channel = channel;
            this.Task = new ChannelFuture<T>(Channel, CompletionSource.Task);
        }

        public IChannel Channel { get; private set; }

        public TaskCompletionSource<T> CompletionSource { get; private set; }

        public ChannelFuture<T> Task { get; private set; }

        public bool TrySetResult(T result)
        {
            return CompletionSource.TrySetResult(result);
        }

        public bool TrySetException(Exception ex)
        {
            return CompletionSource.TrySetException(ex);
        }
    }
}
