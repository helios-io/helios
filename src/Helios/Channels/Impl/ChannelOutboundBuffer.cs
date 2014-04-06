using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Util;
using Helios.Util.Collections;

namespace Helios.Channels.Impl
{
    /// <summary>
    /// Internal buffer for outbound messages - uses a <see cref="ICircularBuffer{T}"/> collection to store messages that
    /// have yet to be flushed to the outbound network connection
    /// </summary>
    public class ChannelOutboundBuffer
    {
        public const int INITIAL_OUTBOUND_CAPACITY = 1000;
        private ICircularBuffer<Entry> _messages = new ConcurrentCircularBuffer<Entry>(INITIAL_OUTBOUND_CAPACITY);

        protected ChannelOutboundBuffer(AbstractChannel channel)
        {
            Channel = channel;
        }

        protected readonly AbstractChannel Channel;
        private bool inFail;
        private AtomicReference<int> writable = 1;
        private AtomicReference<long> totalPendingSize = 0;

        /// <summary>
        /// Increment the pending bytes which will be written at some point
        /// </summary>
        public void IncrementPendingOutboundBytes(int size)
        {
            var channel = this.Channel;
            if (size == 0 || channel == null) return;

            long oldValue = totalPendingSize;
            var newWriteBufferSize = oldValue + size;
            while (!totalPendingSize.CompareAndSet(oldValue, newWriteBufferSize))
            {
                oldValue = totalPendingSize;
                newWriteBufferSize = oldValue + size;
            }


        }

        internal ChannelFuture<bool> AddWrite(NetworkData message)
        {
            var promise = NewPromise();
            if (_messages.Size == _messages.Capacity) //cancel the message at the front of the buffer since we weren't able to deliver it on time
            {
                var cancelledHead = _messages.Dequeue();
                cancelledHead.Cancel();
            }
            _messages.Enqueue(NewEntry(message, promise));
            return promise.Task;
        }

        /// <summary>
        /// Flush all of the messages in this buffer
        /// </summary>
        internal IList<Entry> Flush()
        {
            return _messages.DequeueAll().ToList();
        }

        internal ChannelPromise<bool> NewPromise()
        {
            return new ChannelPromise<bool>(Channel);
        }

        internal static Entry NewEntry(NetworkData message, ChannelPromise<bool> flushPromise)
        {
            return new Entry(){ Message = message.Buffer, FlushedCompletionSource = flushPromise, PendingSize = message.Length};
        }

        #region Static methods

        /// <summary>
        /// Factory method for creating new <see cref="ChannelOutboundBuffer"/> instances
        /// </summary>
        internal static ChannelOutboundBuffer NewBuffer(AbstractChannel channel)
        {
            return new ChannelOutboundBuffer(channel);
        }

        #endregion

        internal class Entry
        {
            public byte[] Message { get; set; }
            public ChannelPromise<bool> FlushedCompletionSource { get; set; }

            /// <summary>
            /// The size of this message in bytes
            /// </summary>
            public int PendingSize { get; set; }

            public bool Cancelled { get; set; }

            /// <summary>
            /// Returns the size in bytes of the message that was cancelled
            /// </summary>
            public int Cancel()
            {
                if (!Cancelled)
                {
                    Cancelled = true;
                    var pSize = PendingSize;
                    Message = Unpooled.EmptyBuffer;
                    FlushedCompletionSource.SetCanceled();
                    return pSize;
                }

                //entry was already cancelled
                return 0;
            }

            /// <summary>
            /// Clear all resources associated with this entry
            /// </summary>
            public void Clear()
            {
                Cancelled = true;
                FlushedCompletionSource = null;
                PendingSize = 0;
                Message = null;
            }
        }
    }
}
