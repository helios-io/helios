using System;
using Helios.Exceptions;
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
        public const int INITIAL_OUTBOUND_CAPACITY = 100;

        protected ChannelOutboundBuffer(AbstractChannel channel)
        {
            Channel = channel;
            buffer = new Entry[INITIAL_OUTBOUND_CAPACITY];
            for (var i = 0; i < buffer.Length; i++)
                buffer[i] = NewEntry();
        }

        protected AbstractChannel Channel;
        private volatile bool inFail;
        private Entry[] buffer;
        private int flushed;
        private int unflushed;
        private int tail;

        private AtomicReference<int> writable = 1;
        private AtomicReference<long> totalPendingSize = 0;

        protected Entry[] entries
        {
            get { return buffer; }
        }

        public void AddMessage(NetworkData message, ChannelPromise<bool> promise)
        {
            int size = message.Length;
            if (size < 0) size = 0;

            Entry e = buffer[tail++];
            e.Message = message.Buffer;
            e.PendingSize = size;
            e.Promise = promise;

            tail &= buffer.Length - 1;

            if (tail == flushed)
            {
                AddCapacity();
            }

            IncrementPendingOutboundBytes(size);
        }

        /// <summary>
        /// Expand the internal buffer
        /// </summary>
        private void AddCapacity()
        {
            var p = flushed;
            var n = buffer.Length;
            var r = n - p; // number of elements to the right of p
            var s = Size;

            var newCapacity = n << 1;
            if (newCapacity < 0)
            {
                throw new InvalidOperationException();
            }

            var e = new Entry[newCapacity];
            Array.Copy(buffer, p, e, 0, r);
            Array.Copy(buffer, 0, e, r, p);
            for (var i = n; i < e.Length; i++)
            {
                e[i] = NewEntry();
            }

            buffer = e;
            flushed = 0;
            unflushed = s;
            tail = n;
        }

        /// <summary>
        /// Mark all messages in this <see cref="ChannelOutboundBuffer"/> as flushed
        /// </summary>
        public void AddFlush()
        {
            unflushed = tail;

            var mask = buffer.Length - 1;
            var i = flushed;
            while (i != unflushed && buffer[i].Message != null)
            {
                var entry = buffer[i];
                if (entry.Promise.Task.IsCanceled)
                {
                    // Was cancelled, so make sure we free up memory and notify about the freed bytes
                    var pending = entry.Cancel();
                    DecrementPendingOutboundBytes(pending);
                }
                i = i + 1 & mask;
            }
        }

        /// <summary>
        /// Increment the pending bytes which will be written at some point
        /// </summary>
        public void IncrementPendingOutboundBytes(int size)
        {
            var channel = Channel;
            if (size == 0 || channel == null) return;

            long oldValue = totalPendingSize;
            var newWriteBufferSize = oldValue + size;
            while (!totalPendingSize.CompareAndSet(oldValue, newWriteBufferSize))
            {
                oldValue = totalPendingSize;
                newWriteBufferSize = oldValue + size;
            }

            var highWaterMark = channel.Config.WriteBufferHighWaterMark;

            if (newWriteBufferSize > highWaterMark)
            {
                if (writable.CompareAndSet(1, 0))
                {
                    channel.Pipeline.FireChannelWritabilityChanged();
                }
            }
        }

        /// <summary>
        /// Decrement the pending bytes which will be written at some point
        /// </summary>
        public void DecrementPendingOutboundBytes(int size)
        {
            var channel = this.Channel;
            if (size == 0 || channel == null) return;

            long oldValue = totalPendingSize;
            var newWriteBufferSize = oldValue - size;
            while (!totalPendingSize.CompareAndSet(oldValue, newWriteBufferSize))
            {
                oldValue = totalPendingSize;
                newWriteBufferSize = oldValue - size;
            }

            int lowWaterMark = channel.Config.WriteBufferLowWaterMark;

            if (newWriteBufferSize == 0 || newWriteBufferSize < lowWaterMark)
            {
                if (writable.CompareAndSet(0,1))
                {
                    channel.Pipeline.FireChannelWritabilityChanged();
                }
            }
        }

        public byte[] Current
        {
            get
            {
                if (IsEmpty) return null;
                var entry = buffer[flushed];
                return entry.Message;
            }
        }

        /// <summary>
        /// Mark the current message as successful and remove it from this <see cref="ChannelOutboundBuffer"/>
        /// </summary>
        /// <returns>true if there are more messages left to process, false otherwise</returns>
        public bool Remove()
        {
            if (IsEmpty) return false;
            var e = buffer[flushed];
            if (e == null || e.Message == null) return false;

            var promise = e.Promise;
            var size = e.PendingSize;

            e.Clear();

            flushed = flushed + 1 & buffer.Length - 1;

            if (!e.Cancelled)
            {
                //only release message, notify, and decrement if it was not canceled before
                SafeSuccess(e.Promise);
                DecrementPendingOutboundBytes(size);
            }

            return true;
        }

        /// <summary>
        /// Mark the current message as unsuccessful and remove it from this <see cref="ChannelOutboundBuffer"/>.
        /// </summary>
        /// <returns>true if there are more messages left to process, false otherwise</returns>
        public bool Remove(Exception cause)
        {
            if (IsEmpty) return false;
            var e = buffer[flushed];
            if (e == null || e.Message == null) return false;

            var promise = e.Promise;
            var size = e.PendingSize;

            e.Clear();

            flushed = flushed + 1 & buffer.Length - 1;

            if (!e.Cancelled)
            {
                //only release message, notify, and decrement if it was not canceled before
                SafeFail(promise, cause);
                DecrementPendingOutboundBytes(size);
            }

            return true;
        }

        public int Size { get { return unflushed - flushed & buffer.Length - 1; } }

        public bool IsEmpty { get { return unflushed == flushed; } }

        /// <summary>
        /// Fail all unflushed messages with the given <see cref="cause"/>
        /// </summary>
        public void FailFlushed(Exception cause)
        {
            if (inFail) return; //don't re-enter this method

            try
            {
                inFail = true;
                for (;;)
                {
                    if (!Remove(cause)) break;
                }
            }
            finally
            {
                inFail = false;
            }
        }

        /// <summary>
        /// Fail all pending messages with the given <see cref="HeliosConnectionException"/>
        /// </summary>
        public void Close(HeliosConnectionException cause)
        {
            if (inFail)
            { 
                //schedule to run later
                Channel.EventLoop.Execute(() => Close(cause));
                return;
            }

            inFail = true;

            if (Channel.IsOpen)
            {
                throw new InvalidOperationException("Close() must be invoked after the channel is closed.");
            }

            if (!IsEmpty)
            {
                throw new InvalidOperationException("Close() must be invoked after all flushed writes are handled.");
            }

            //Release all unflushed messages
            var unflushedCount = tail - unflushed & buffer.Length - 1;
            try
            {
                for (var i = 0; i < unflushedCount; i++)
                {
                    var e = buffer[unflushed + i & buffer.Length - 1];

                    //Just a decrease; do not trigger any events via decrementPendingOutboundBytes()
                    var size = e.PendingSize;
                    long oldValue = totalPendingSize;
                    long newWriteBufferSize = oldValue - size;
                    while (!totalPendingSize.CompareAndSet(oldValue, newWriteBufferSize))
                    {
                        oldValue = totalPendingSize;
                        newWriteBufferSize = oldValue - size;
                    }

                    e.PendingSize = 0;
                    if (!e.Cancelled)
                    {
                        SafeFail(e.Promise, cause);
                    }
                    e.Message = null;
                    e.Promise = null;
                }
            }
            finally
            {
                tail = unflushed;
                inFail = false;
            }
            Recycle();
        }

        private void Recycle()
        {
            if (buffer.Length > INITIAL_OUTBOUND_CAPACITY)
            {
                var e = new Entry[INITIAL_OUTBOUND_CAPACITY];
                Array.Copy(buffer, 0, e, 0, INITIAL_OUTBOUND_CAPACITY);
                buffer = e;
            }

            flushed = 0;
            unflushed = 0;
            tail = 0;

            // Set the channel to null so it can be GC'ed ASAP
            Channel = null;

            totalPendingSize = 0;
            writable = 1;

        }

        internal ChannelPromise<bool> NewPromise()
        {
            return new ChannelPromise<bool>(Channel);
        }

        static Entry NewEntry(NetworkData message, ChannelPromise<bool> flushPromise)
        {
            return new Entry(){ Message = message.Buffer, Promise = flushPromise, PendingSize = message.Length};
        }

        static Entry NewEntry()
        {
            return new Entry();
        }

        #region Static methods

        /// <summary>
        /// Factory method for creating new <see cref="ChannelOutboundBuffer"/> instances
        /// </summary>
        internal static ChannelOutboundBuffer NewBuffer(AbstractChannel channel)
        {
            return new ChannelOutboundBuffer(channel);
        }

        internal static void SafeSuccess(ChannelPromise<bool> promise)
        {
            if (!(promise is VoidChannelPromise) && !promise.TrySetResult(true))
            {
                //add logging here
            }
        }

        internal static void SafeFail(ChannelPromise<bool> promise, Exception cause)
        {
            if (!(promise is VoidChannelPromise) && !promise.TrySetException(cause))
            {
                //add logging here
            }
        }

        #endregion

        protected class Entry
        {
            public byte[] Message { get; set; }
            public ChannelPromise<bool> Promise { get; set; }

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
                    Promise.SetCanceled();
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
                Promise = null;
                PendingSize = 0;
                Message = null;
            }
        }
    }
}
