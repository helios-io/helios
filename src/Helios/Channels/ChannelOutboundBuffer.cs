// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        private static readonly ThreadLocalByteBufferList NioBuffers = new ThreadLocalByteBufferList();

        /// <summary>
        ///     Callback used to indicate that the channel is going to become writeable or unwriteable
        /// </summary>
        private readonly Action _fireChannelWritabilityChanged;

        private readonly IChannel _channel;

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

        private int _writeBufferHighWaterMark => _channel.Configuration.WriteBufferHighWaterMark;
        private int _writeBufferLowWaterMark => _channel.Configuration.WriteBufferLowWaterMark;

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
            var entry = Entry.NewInstance(message, size, promise);
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
                // only release message, notify and decrement if it was not canceled before.
                ReferenceCountUtil.SafeRelease(msg);
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
                // only release message, notify and decrement if it was not canceled before.
                ReferenceCountUtil.SafeRelease(msg);
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

        public void IncrementPendingOutboundBytes(long size)
        {
            if (size == 0)
                return;

            var newWriteBufferSize = Interlocked.Add(ref _totalPendingSize, size);
            if (newWriteBufferSize >= _writeBufferHighWaterMark)
            {
                SetUnwritable();
            }
        }

        public void DecrementPendingOutboundBytes(long size)
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
                        ReferenceCountUtil.SafeRelease(e.Message);
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
        ///     Removes the fully written entries and update the reader index of the partially written entry.
        ///     This operation assumes all messages in this buffer is <see cref="IByteBuf"/>.
        /// </summary>
        public void RemoveBytes(long writtenBytes)
        {
            while (true)
            {
                object msg = this.Current;
                if (!(msg is IByteBuf))
                {
                    Contract.Assert(writtenBytes == 0);
                    break;
                }

                var buf = (IByteBuf) msg;
                int readerIndex = buf.ReaderIndex;
                int readableBytes = buf.WriterIndex - readerIndex;

                if (readableBytes <= writtenBytes)
                {
                    if (writtenBytes != 0)
                    {
                        writtenBytes -= readableBytes;
                    }
                    this.Remove();
                }
                else
                {
                    // readableBytes > writtenBytes
                    if (writtenBytes != 0)
                    {
                        buf.SetReaderIndex(readerIndex + (int) writtenBytes);
                    }
                    break;
                }
            }
            this.ClearNioBuffers();
        }

        private void ClearNioBuffers() => NioBuffers.Value.Clear();

        ///
        ///Returns an array of direct NIO buffers if the currently pending messages are made of {@link ByteBuf} only.
        ///{@link #nioBufferCount()} and {@link #nioBufferSize()} will return the number of NIO buffers in the returned
        ///array and the total number of readable bytes of the NIO buffers respectively.
        ///<p>
        ///Note that the returned array is reused and thus should not escape
        ///{@link AbstractChannel#doWrite(ChannelOutboundBuffer)}.
        ///Refer to {@link NioSocketChannel#doWrite(ChannelOutboundBuffer)} for an example.
        ///</p>
        ///
        public List<ArraySegment<byte>> GetNioBuffers()
        {
            long nioBufferSize = 0;
            InternalThreadLocalMap threadLocalMap = InternalThreadLocalMap.Get();
            List<ArraySegment<byte>> nioBuffers = NioBuffers.Get(threadLocalMap);
            Entry entry = _flushedEntry;
            while (IsFlushedEntry(entry) && entry.Message is IByteBuf)
            {
                if (!entry.Cancelled)
                {
                    var buf = (IByteBuf) entry.Message;
                    int readerIndex = buf.ReaderIndex;
                    int readableBytes = buf.WriterIndex - readerIndex;

                    if (readableBytes > 0)
                    {
                        if (int.MaxValue - readableBytes < nioBufferSize)
                        {
                            // If the nioBufferSize + readableBytes will overflow an Integer we stop populate the
                            // ByteBuffer array. This is done as bsd/osx don't allow to write more bytes then
                            // Integer.MAX_VALUE with one writev(...) call and so will return 'EINVAL', which will
                            // raise an IOException. On Linux it may work depending on the
                            // architecture and kernel but to be safe we also enforce the limit here.
                            // This said writing more the Integer.MAX_VALUE is not a good idea anyway.
                            //
                            // See also:
                            // - https://www.freebsd.org/cgi/man.cgi?query=write&sektion=2
                            // - http://linux.die.net/man/2/writev
                            break;
                        }
                        nioBufferSize += readableBytes;
                        int count = entry.Count;
                        if (count == -1)
                        {
                            //noinspection ConstantValueVariableUse
                            entry.Count = count = buf.IoBufferCount;
                        }
                        if (count == 1)
                        {
                            ArraySegment<byte> nioBuf = entry.Buffer;
                            if (nioBuf.Array == null)
                            {
                                // cache ByteBuffer as it may need to create a new ByteBuffer instance if its a
                                // derived buffer
                                entry.Buffer = nioBuf = buf.GetIoBuffer(readerIndex, readableBytes);
                            }
                            nioBuffers.Add(nioBuf);
                        }
                        else
                        {
                            ArraySegment<byte>[] nioBufs = entry.Buffers;
                            if (nioBufs == null)
                            {
                                // cached ByteBuffers as they may be expensive to create in terms
                                // of Object allocation
                                entry.Buffers = nioBufs = buf.GetIoBuffers();
                            }
                            foreach (ArraySegment<byte> b in nioBufs)
                            {
                                nioBuffers.Add(b);
                            }
                        }
                    }
                }
                entry = entry.Next;
            }
            this.NioBufferSize = nioBufferSize;

            return nioBuffers;
        }

        /**
         * Returns the number of bytes that can be written out of the {@link ByteBuffer} array that was
         * obtained via {@link #nioBuffers()}. This method <strong>MUST</strong> be called after {@link #nioBuffers()}
         * was called.
         */
        public long NioBufferSize { get; private set; }

        /// <summary>
        ///     Call {@link IMessageProcessor#processMessage(Object)} for each flushed message
        ///     in this {@link ChannelOutboundBuffer} until {@link IMessageProcessor#processMessage(Object)}
        ///     returns {@code false} or there are no more flushed messages to process.
        /// </summary>
        private bool IsFlushedEntry(Entry e) => e != null && e != this._unflushedEntry;

        /// <summary>
        ///     Represents an entry inside the <see cref="ChannelOutboundBuffer" />
        /// </summary>
        private sealed class Entry
        {
            private static readonly ThreadLocal<ObjectPool<Entry>> Pool =
                new ThreadLocal<ObjectPool<Entry>>(() => new ObjectPool<Entry>(handle => new Entry(handle)));

            public bool Cancelled;
            public object Message;
            public ArraySegment<byte>[] Buffers;
            public ArraySegment<byte> Buffer;
            public Entry Next; //linked list
            public int PendingSize;
            public int Count = -1;
            public TaskCompletionSource Promise;
            private readonly PoolHandle<Entry> handle;

            private Entry(PoolHandle<Entry> handle)
            {
                this.handle = handle;
            }

            public static Entry NewInstance(object message, int size, TaskCompletionSource promise)
            {
                var entry = Pool.Value.Take();
                entry.Message = message;
                entry.PendingSize = size;
                entry.Promise = promise;
                return entry;
            }

            public int Cancel()
            {
                if (!Cancelled)
                {
                    Cancelled = true;
                    var pSize = PendingSize;

                    // release message and replace with an empty buffer
                    ReferenceCountUtil.SafeRelease(Message);
                    Message = Unpooled.Empty;
                    PendingSize = 0;
                    Buffers = null;
                    Buffer = new ArraySegment<byte>();
                    return pSize;
                }
                return 0;
            }

            public void Recycle()
            {
                Buffers = null;
                Buffer = new ArraySegment<byte>();
                PendingSize = 0;
                Message = null;
                Next = null;
                Promise = null;
                Count = -1;
                Cancelled = false;
                handle.Free(this);
            }

            public Entry RecycleAndGetNext()
            {
                var next = Next;
                Recycle();
                return next;
            }
        }

        private sealed class ThreadLocalByteBufferList : FastThreadLocal<List<ArraySegment<byte>>>
        {
            protected override List<ArraySegment<byte>> GetInitialValue()
            {
                return new List<ArraySegment<byte>>();
            }
        }
    }
}