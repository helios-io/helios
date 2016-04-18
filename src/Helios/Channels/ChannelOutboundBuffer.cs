using Helios.Buffers;
using Helios.Channels;
using Helios.Concurrency;
using Helios.Logging;

namespace Helios.Channels { 

    /// <summary>
    /// Internal data structure used by <see cref="IChannel"/> implementations to store its pending
    /// outbound write requests.
    /// 
    /// Methods are designed to be called from the I/O thread, except for 
    /// </summary>
    public sealed class ChannelOutboundBuffer
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<ChannelOutboundBuffer>();

        // The number of entries that are not yet written
        private int _flushed;

        /// <summary>
        /// Returns <c>true</c> if all messages have been written to the transport, <c>false</c>
        /// if there are messages still waiting.
        /// </summary>
        public bool IsEmpty => _flushed == 0;

        private readonly IChannel _channel;

        // Entry(flushedEntry) --> ... Entry(unflushedEntry) --> ... Entry(tailEntry)
        //
        // The Entry that is the first in the linked-list structure that was flushed
        Entry _flushedEntry;
        // The Entry which is the first unflushed in the linked-list structure
        Entry _unflushedEntry;
        // The Entry which represents the tail of the buffer
        Entry _tailEntry;

        // Currently in a "failed to write" mode
        private bool _inFail;

        private long _totalPendingSize;

        private volatile int _unwritable;

        internal ChannelOutboundBuffer(IChannel channel)
        {
            _channel = channel;
        }

        public object Current
        {
            get
            {
                var entry = _flushedEntry;
                if (_flushedEntry == null)
                {
                    return null;
                }
                return entry.Message;
            }
        }

        public bool Remove()
        {
            var entry = _flushedEntry;
            if (_flushedEntry == null)
            {
                return false;
            }
            var msg = entry.Message;
            var promise = entry.Promise;
            var size = entry.PendingSize;

            RemoveEntry(entry);
            if (!entry.Cancelled)
            {
                // TODO: reference counting
                PromiseUtil.SafeSetSuccess(promise, Logger);
            }

            // recycle the entry
            entry.Recycle();

            return true;
        }

        private void RemoveEntry(Entry entry)
        {
            if (--_flushed == 0)
            {
                // finished processing everything
                _flushedEntry = null;
                if (entry == _tailEntry)
                {
                    _tailEntry = null;
                    _unflushedEntry = null;
                }
            }
            else
            {
                _flushedEntry = entry.Next;
            }
        }

        #region Linked List implementation

        sealed class Entry
        {
            public Entry Next;
            public object Message;
            public TaskCompletionSource Promise;
            public int PendingSize;
            public int Total;
            public bool Cancelled;

            private Entry() { }

            /// <summary>
            /// Cancels this write and returns the number of bytes which will not be written.
            /// </summary>
            /// <returns></returns>
            public int Cancel()
            {
                if (!Cancelled)
                {
                    Cancelled = true;
                    var pendingSize = PendingSize;

                    // TODO: reference counting for messages that support it
                    Message = Unpooled.Empty;

                    PendingSize = 0;
                    Total = 0;
                    return pendingSize;
                }
                return 0;
            }

            public void Recycle()
            {
                // TODO: object pooling
            }

            public Entry RecycleAndGetNext()
            {
                var next = Next;
                Recycle();
                return next;
            }
        }

        #endregion
    }
}