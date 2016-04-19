using System;
using System.Threading;
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

        /// <summary>
        /// Returns <c>true</c> if and only if <see cref="TotalPendingWriteBytes()"/>
        /// </summary>
        public bool IsWritable => _unwritable == 0;

        /// <summary>
        /// Returns the number of flushed messages in this <see cref="ChannelOutboundBuffer"/>
        /// </summary>
        public int Count => _flushed;

        public long TotalPendingWriteBytes()
        {
            return Thread.VolatileRead(ref _totalPendingSize);
        }

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

        /// <summary>
        /// Add given message to this {@link ChannelOutboundBuffer}. The given {@link ChannelPromise} will be notified once
        /// the message was written.
        /// </summary>
        public void AddMessage(object msg, int size, TaskCompletionSource promise)
        {
            var entry = Entry.NewInstance(msg, size, Total(msg), promise);
            if (_tailEntry == null)
            {
                _flushedEntry = null;
                _tailEntry = entry;
            }
            else
            {
                var tail = _tailEntry;
                tail.Next = entry;
                _tailEntry = entry;
            }
            if (_unflushedEntry == null)
            {
                _unflushedEntry = entry;
            }

            // increment pending bytes after adding message to the unflushed arrays.
            IncrementPendingOutboundBytes(size, false);
        }

        /// <summary>
        /// Add a flush to this {@link ChannelOutboundBuffer}. This means all previous added messages are marked as flushed
        /// and so you will be able to handle them.
        /// </summary>
        public void AddFlush()
        {
            // There is no need to process all entries if there was already a flush before and no new messages
            // where added in the meantime.

            Entry entry = _unflushedEntry;
            if (entry != null)
            {
                if (_flushedEntry == null)
                {
                    // there is no flushedEntry yet, so start with the entry
                    _flushedEntry = entry;
                }
                do
                {
                    _flushed++;
                    if (!entry.Promise.SetUncancellable())
                    {
                        // Was cancelled so make sure we free up memory and notify about the freed bytes
                        var pending = entry.Cancel();
                        DecrementPendingOutboundBytes(pending, false, true);
                    }
                    entry = entry.Next;
                }
                while (entry != null);

                // All flushed so reset unflushedEntry
                _unflushedEntry = null;
            }
        }

        // TODO: why not use message size estimator?
        public static long Total(object msg)
        {
            if (msg is IByteBuf)
            {
                return ((IByteBuf)msg).ReadableBytes;
            }

            return -1;
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

        /// <summary>
        /// Will remove the current message, mark its <see cref="TaskCompletionSource"/> as failure using the given <see cref="Exception"/>
        /// and return <c>true</c>. If no flushed message exists at the time this method is called it will return
        /// <c>false</c> to signal that no more messages are ready to be handled.
        /// </summary>
        public bool Remove(Exception cause)
        {
            return Remove0(cause, true);
        }

        private bool Remove0(Exception cause, bool notifyWritability)
        {
            var e = _flushedEntry;
            if (e == null)
            {
                return false;
            }
            var msg = e.Message;

            var promise = e.Promise;
            var size = e.PendingSize;

            RemoveEntry(e);

            if (!e.Cancelled)
            {
                // only release message, fail and decrement if it was not canceled before.
                // TODO: referencing counting

                PromiseUtil.SafeSetFailure(promise, cause, Logger);
                if (promise != TaskCompletionSource.Void && !promise.TrySetException(cause))
                {
                    Logger.Warning("Failed to mark a promise as failure because it's done already: {0}; Cause: {1}", promise, cause);
                }
                DecrementPendingOutboundBytes(size, false, notifyWritability);
            }

            // recycle the entry
            e.Recycle();

            return true;
        }

        /// <summary>
        /// Increment the pending bytes which will be written at some point.
        /// This method is thread-safe!
        /// </summary>
        internal void IncrementPendingOutboundBytes(long size)
        {
            IncrementPendingOutboundBytes(size, true);
        }

        private void IncrementPendingOutboundBytes(long size, bool invokeLater)
        {
            if (size == 0)
            {
                return;
            }

            long newWriteBufferSize = Interlocked.Add(ref _totalPendingSize, size);
            if (newWriteBufferSize >= _channel.Configuration.WriteBufferHighWaterMark)
            {
                SetUnwritable(invokeLater);
            }
        }

        /// <summary>
        /// Decrement the pending bytes which will be written at some point.
        /// This method is thread-safe!
        /// </summary>
        internal void DecrementPendingOutboundBytes(long size)
        {
            this.DecrementPendingOutboundBytes(size, true, true);
        }

        private void DecrementPendingOutboundBytes(long size, bool invokeLater, bool notifyWritability)
        {
            if (size == 0)
            {
                return;
            }

            long newWriteBufferSize = Interlocked.Add(ref _totalPendingSize, -size);
            if (notifyWritability && (newWriteBufferSize == 0
                || newWriteBufferSize <= _channel.Configuration.WriteBufferLowWaterMark))
            {
                this.SetWritable(invokeLater);
            }
        }

        void SetWritable(bool invokeLater)
        {
            while (true)
            {
                int oldValue = _unwritable;
                int newValue = oldValue & ~1;
                if (Interlocked.CompareExchange(ref _unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue != 0 && newValue == 0)
                    {
                        this.FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }

        void SetUnwritable(bool invokeLater)
        {
            while (true)
            {
                int oldValue = _unwritable;
                int newValue = oldValue | 1;
                if (Interlocked.CompareExchange(ref _unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue == 0 && newValue != 0)
                    {
                        FireChannelWritabilityChanged(invokeLater);
                    }
                    break;
                }
            }
        }

        void FireChannelWritabilityChanged(bool invokeLater)
        {
            IChannelPipeline pipeline = _channel.Pipeline;
            if (invokeLater)
            {
                // todo: allocation check
                _channel.EventLoop.Execute(p => ((IChannelPipeline)p).FireChannelWritabilityChanged(), pipeline);
            }
            else
            {
                pipeline.FireChannelWritabilityChanged();
            }
        }

        internal void FailFlushed(Exception cause, bool notify)
        {
            // Make sure that this method does not reenter.  A listener added to the current promise can be notified by the
            // current thread in the tryFailure() call of the loop below, and the listener can trigger another fail() call
            // indirectly (usually by closing the channel.)
            //
            // See https://github.com/netty/netty/issues/1501
            if (_inFail)
            {
                return;
            }

            try
            {
                _inFail = true;
                for (;;)
                {
                    if (!this.Remove0(cause, notify))
                    {
                        break;
                    }
                }
            }
            finally
            {
                _inFail = false;
            }
        }

        internal void Close(ClosedChannelException cause)
        {
            if (_inFail)
            {
                _channel.EventLoop.Execute((buf, ex) => ((ChannelOutboundBuffer)buf).Close((ClosedChannelException)ex),
                    this, cause);
                return;
            }

            _inFail = true;

            if (_channel.Open)
            {
                throw new InvalidOperationException("close() must be invoked after the channel is closed.");
            }

            if (!this.IsEmpty)
            {
                throw new InvalidOperationException("close() must be invoked after all flushed writes are handled.");
            }

            // Release all unflushed messages.
            try
            {
                Entry e = _unflushedEntry;
                while (e != null)
                {
                    // Just decrease; do not trigger any events via DecrementPendingOutboundBytes()
                    int size = e.PendingSize;
                    Interlocked.Add(ref _totalPendingSize, -size);

                    if (!e.Cancelled)
                    {
                        //TODO: referencing counting
                        PromiseUtil.SafeSetFailure(e.Promise, cause, Logger);
                        if (e.Promise != TaskCompletionSource.Void && !e.Promise.TrySetException(cause))
                        {
                            Logger.Warning("Failed to mark a promise as failure because it's done already: {0}; Cause: {1}", e.Promise, cause);
                        }
                    }
                    e = e.RecycleAndGetNext();
                }
            }
            finally
            {
                _inFail = false;
            }
        }

        #region Linked List implementation

        sealed class Entry
        {
            public Entry Next;
            public object Message;
            public TaskCompletionSource Promise;
            public int PendingSize;
            public long Total;
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

            public static Entry NewInstance(object msg, int size, long total, TaskCompletionSource promise)
            {
                // TODO: object pooling
                Entry entry = new Entry();
                entry.Message = msg;
                entry.PendingSize = size;
                entry.Total = total;
                entry.Promise = promise;
                return entry;
            }
        }

        #endregion
    }
}