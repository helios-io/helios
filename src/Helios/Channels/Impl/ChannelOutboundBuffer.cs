using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helios.Net;
using Helios.Util.Collections;

namespace Helios.Channels.Impl
{
    /// <summary>
    /// Internal buffer for outbound messages - uses a <see cref="ICircularBuffer{T}"/> collection to store messages that
    /// have yet to be flushed to the outbound network connection
    /// </summary>
    internal class ChannelOutboundBuffer
    {
        public const int INITIAL_OUTBOUND_CAPACITY = 1000;
        protected ICircularBuffer<Entry> _messages = new ConcurrentCircularBuffer<Entry>(INITIAL_OUTBOUND_CAPACITY);

        public TaskCompletionSource<bool> AddWrite(NetworkData message)
        {
            var promise = NewPromise();
            if (_messages.Size == _messages.Capacity) //cancel the message at the front of the buffer since we weren't able to deliver it on time
            {
                var cancelledHead = _messages.Dequeue();
                cancelledHead.Cancel();
            }
            _messages.Enqueue(NewEntry(message, promise));
            return promise;
        }

        /// <summary>
        /// Flush all of the messages in this buffer
        /// </summary>
        public IList<Entry> Flush()
        {
            return _messages.DequeueAll().ToList();
        }

        internal static TaskCompletionSource<bool> NewPromise()
        {
            return new TaskCompletionSource<bool>(TaskCreationOptions.PreferFairness);
        }

        internal static Entry NewEntry(NetworkData message, TaskCompletionSource<bool> flushPromise)
        {
            return new Entry(){ Message = message.Buffer, FlushedCompletionSource = flushPromise, PendingSize = message.Length};
        }

        internal class Entry
        {
            public byte[] Message { get; set; }
            public TaskCompletionSource<bool> FlushedCompletionSource { get; set; }

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
