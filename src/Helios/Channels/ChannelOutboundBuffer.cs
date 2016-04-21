using System;
using System.Threading;
using Helios.Buffers;
using Helios.Concurrency;
using Helios.Logging;
using Helios.Util;

namespace Helios.Channels
{
    /// <summary>
    ///     Outbound buffer used to store messages queued for outbound delivery
    ///     on a given channel. These messages will be dequed and flushed to the
    ///     underlying socket or transport.
    /// </summary>
    public sealed class ChannelOutboundBuffer
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<ChannelOutboundBuffer>();

        private int _writeBufferHighWaterMark => _channel.Configuration.WriteBufferHighWaterMark;
        private int _writeBufferLowWaterMark => _channel.Configuration.WriteBufferLowWaterMark;

        private IChannel _channel;

        /// <summary>
        ///     Callback used to indicate that the channel is going to become writeable or unwriteable
        /// </summary>
        private readonly Action _fireChannelWritabilityChanged;

        /// <summary>
        ///     Number of flushed entries not yet written
        /// </summary>
        private int _flushed;

        // Entry(flushedEntry) --> ... Entry(unflushedEntry) --> ... Entry(tailEntry)
        //
        // The Entry that is the first in the linked-list structure that was flushed
        private Entry _flushedEntry;

        private bool _inFail;
        // The Entry which represents the tail of the buffer
        private Entry _tailEntry;
        private long _totalPendingSize;
        // The Entry which is the first unflushed in the linked-list structure
        private Entry _unflushedEntry;

        private volatile int _unwritable;

        public ChannelOutboundBuffer(IChannel channel,
            Action fireChannelWritabilityChanged)
        {
            _channel = channel;
            _fireChannelWritabilityChanged = fireChannelWritabilityChanged;
        }

        /// <summary>
        ///     Return the current message to write or <c>null</c> if nothing was flushed before and so is ready to be written.
        /// </summary>
        public object Current
        {
            get
            {
                var entry = _flushedEntry;
                return entry?.Message;
            }
        }

        /// <summary>
        ///     Returns <c>true</c> if
        /// </summary>
        public bool IsWritable => _unwritable == 0;

        /// <summary>
        ///     Returns the number of flushed messages
        /// </summary>
        public int Count => _flushed;

        /// <summary>
        ///     Returns <c>true</c> if there are flushed messages in this buffer. <c>false</c> otherwise.
        /// </summary>
        public bool IsEmpty => _flushed == 0;

        /// <summary>
        ///     The total number of bytes waiting to be written
        /// </summary>
        public long TotalPendingWriteBytes => Thread.VolatileRead(ref _totalPendingSize);

        /// <summary>
        ///     Number of bytes we can write before we become unwriteable
        /// </summary>
        public long BytesBeforeUnwritable
        {
            get
            {
                var bytes = _writeBufferHighWaterMark - TotalPendingWriteBytes;
                if (bytes > 0)
                    return IsWritable ? bytes : 0;
                return 0;
            }
        }

        public long BytesBeforeWritable
        {
            get
            {
                var bytes = TotalPendingWriteBytes - _writeBufferLowWaterMark;
                if (bytes > 0)
                    return IsWritable ? 0 : bytes;
                return 0;
            }
        }

        /// <summary>
        ///     Add a given message to this <see cref="ChannelOutboundBuffer" />.
        /// </summary>
        /// <param name="message">The message that will be written.</param>
        /// <param name="size">The size of the message.</param>
        /// <param name="promise">A <see cref="TaskCompletionSource" /> that will be set once message was written.</param>
        public void AddMessage(object message, int size, TaskCompletionSource promise)
        {
            var entry = Entry.NewInstance(message, size, Total(message), promise);
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

            IncrementPendingOutboundBytes(size);
        }

        /// <summary>
        ///     Flush all current messages in the outbound buffer
        /// </summary>
        public void AddFlush()
        {
            var entry = _unflushedEntry;
            if (entry != null)
            {
                if (_flushedEntry == null)
                {
                    _flushedEntry = entry;
                }
                do
                {
                    _flushed++;
                    if (!entry.Promise.SetUncancellable())
                    {
                        // write was cancelled, so free up allocated memory and notify about the freed bytes
                        var pending = entry.Cancel();
                        DecrementPendingOutboundBytes(pending);
                    }
                    entry = entry.Next;
                } while (entry != null);

                // All flushed, so reset the unflushed entry
                _unflushedEntry = null;
            }
        }

        public bool Remove()
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
                // todo: reference counting
                PromiseUtil.SafeSetSuccess(promise, Logger);
                DecrementPendingOutboundBytes(size, true);
            }

            e.Recycle();
            return true;
        }

        public bool Remove(Exception cause)
        {
            return Remove(cause, true);
        }

        private bool Remove(Exception cause, bool notifyWritability)
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
                // todo: reference counting
                PromiseUtil.SafeSetFailure(promise, cause, Logger);
                DecrementPendingOutboundBytes(size, notifyWritability);
            }

            e.Recycle();
            return true;
        }

        private void RemoveEntry(Entry e)
        {
            if (--_flushed == 0)
            {
                // processed everything
                _flushedEntry = null;
                if (e == _tailEntry)
                {
                    _tailEntry = null;
                    _unflushedEntry = null;
                }
            }
            else
            {
                _flushedEntry = e.Next;
            }
        }

        private void IncrementPendingOutboundBytes(long size)
        {
            if (size == 0)
                return;

            var newWriteBufferSize = Interlocked.Add(ref _totalPendingSize, size);
            if (newWriteBufferSize >= _writeBufferHighWaterMark)
            {
                SetUnwritable();
            }
        }

        private void DecrementPendingOutboundBytes(long size)
        {
            DecrementPendingOutboundBytes(size, true);
        }

        private void DecrementPendingOutboundBytes(long size, bool notifyWritability)
        {
            if (size == 0)
                return;
            var newWriteBufferSize = Interlocked.Add(ref _totalPendingSize, -size);
            if (notifyWritability && (newWriteBufferSize == 0 || newWriteBufferSize <= _writeBufferLowWaterMark))
            {
                SetWritable();
            }
        }

        private void SetUnwritable()
        {
            while (true)
            {
                var oldValue = _unwritable;
                var newValue = oldValue | 1;
                if (Interlocked.CompareExchange(ref _unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue == 0 && newValue != 0)
                    {
                        _fireChannelWritabilityChanged();
                    }
                    break;
                }
            }
        }

        private void SetWritable()
        {
            while (true)
            {
                var oldValue = _unwritable;
                var newValue = oldValue & ~1;
                if (Interlocked.CompareExchange(ref _unwritable, newValue, oldValue) == oldValue)
                {
                    if (oldValue != 0 && newValue == 0)
                    {
                        _fireChannelWritabilityChanged();
                    }
                    break;
                }
            }
        }

        private static long Total(object message)
        {
            var buf = message as IByteBuf;
            if (buf != null)
                return buf.ReadableBytes;
            return -1;
        }

        internal void FailFlushed(Exception cause, bool notify)
        {
            if (_inFail)
            {
                return;
            }

            try
            {
                _inFail = true;
                while (true)
                {
                    if (!Remove(cause, notify))
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
                return;
            }

            if (!IsEmpty)
            {
                throw new InvalidOperationException("close() must be called after all flushed writes are handled.");
            }

            _inFail = true;

            try
            {
                var e = _unflushedEntry;
                while (e != null)
                {
                    // No triggering anymore events, as we are shutting down
                    if (!e.Cancelled)
                    {
                        // todo: referencing counting
                        PromiseUtil.SafeSetFailure(e.Promise, cause, Logger);
                    }
                    e = e.RecycleAndGetNext();
                }
            }
            finally
            {
                _inFail = false;
            }
        }

        /// <summary>
        ///     Represents an entry inside the <see cref="ChannelOutboundBuffer" />
        /// </summary>
        private sealed class Entry
        {
            private static readonly ThreadLocal<ObjectPool<Entry>> Pool =
                new ThreadLocal<ObjectPool<Entry>>(() => new ObjectPool<Entry>(() => new Entry()));

            public bool Cancelled;
            public object Message;
            public Entry Next; //linked list
            public int PendingSize;
            public TaskCompletionSource Promise;

            public long Total;

            private Entry()
            {
            }

            public static Entry NewInstance(object message, int size, long total, TaskCompletionSource promise)
            {
                var entry = Pool.Value.Take();
                entry.Message = message;
                entry.PendingSize = size;
                entry.Total = total;
                entry.Promise = promise;
                return entry;
            }

            public int Cancel()
            {
                if (!Cancelled)
                {
                    Cancelled = true;
                    var pSize = PendingSize;

                    // TODO: message reference counting (optional)
                    Message = Unpooled.Empty;
                    PendingSize = 0;
                    Total = 0;
                    return pSize;
                }
                return 0;
            }

            public void Recycle()
            {
                Total = 0;
                PendingSize = 0;
                Message = null;
                Next = null;
                Promise = null;
                Cancelled = false;
                Pool.Value.Free(this);
            }

            public Entry RecycleAndGetNext()
            {
                var next = Next;
                Recycle();
                return next;
            }
        }
    }
}