using System;
using System.Threading.Tasks;

namespace Helios.Channels
{
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

        public virtual bool TrySetResult(T result)
        {
            return CompletionSource.TrySetResult(result);
        }

        public virtual bool TrySetCancelled()
        {
            return CompletionSource.TrySetCanceled();
        }

        public virtual bool TrySetException(Exception ex)
        {
            return CompletionSource.TrySetException(ex);
        }

        public virtual void SetResult(T result)
        {
            CompletionSource.SetResult(result);
        }

        public virtual void SetCanceled()
        {
            CompletionSource.SetCanceled();
        }

        public virtual void SetException(Exception ex)
        {
            CompletionSource.SetException(ex);
        }
    }
}