// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Helios.Buffers;
using Helios.Channels;
using Helios.Util;

namespace Helios.Codecs
{
    /// <summary>
    ///     Used for providing message framing capabilities for inbound data
    /// </summary>
    public abstract class ByteToMessageDecoder : ChannelHandlerAdapter
    {
        /// <summary>
        ///     Cumulation function used for handling merging together <see cref="IByteBuf" />s from partial reads
        /// </summary>
        /// <param name="alloc">The allocator used for creating / managing buffers</param>
        /// <param name="cumulation">The existing cumulative buffer</param>
        /// <param name="input">The input buffer from the current read</param>
        /// <returns>A combined cumulative buffer</returns>
        public delegate IByteBuf Cumulator(IByteBufAllocator alloc, IByteBuf cumulation, IByteBuf input);

        public static readonly Cumulator Merge = (alloc, cumulation, input) =>
        {
            IByteBuf buffer;
            buffer = cumulation.WriterIndex > cumulation.MaxCapacity - input.ReadableBytes
                ? ExpandCumulation(alloc, cumulation, input.ReadableBytes)
                : // expand the buffer
                cumulation; // or, use the original since input will fit inside
            buffer.WriteBytes(input);
            input.Release();
            return buffer;
        };

        private IByteBuf _cumulation;
        private Cumulator _cumulator = Merge;
        private bool _decodeWasNull;
        private int _discardAfterReads = 16;
        private bool _first;
        private int _numReads;

        public bool IsSingleDecode { get; private set; }

        /// <summary>
        ///     Returns the internal cumulative buffer of this decoder. Use at your own risk.
        /// </summary>
        protected IByteBuf InternalBuffer
        {
            get
            {
                if (_cumulation != null)
                    return _cumulation;
                return Unpooled.Empty;
            }
        }

        protected int ActualReadableBytes => InternalBuffer.ReadableBytes;

        /// <summary>
        ///     Expand the existing cumulative <see cref="IByteBuf" />.
        /// </summary>
        private static IByteBuf ExpandCumulation(IByteBufAllocator alloc, IByteBuf cumulation, int readable)
        {
            var old = cumulation;
            cumulation = alloc.Buffer(old.ReadableBytes + readable);
            cumulation.WriteBytes(old);
            old.Release();
            return cumulation;
        }

        /// <summary>
        ///     When set to <c>true</c> then only one message is decoded on each <see cref="IChannelHandler.ChannelRead" />.
        ///     Defaults to <c>false</c> for performance reasons.
        /// </summary>
        /// <param name="singleDecode">The toggle for single decoding</param>
        /// <remarks>May be useful if oyu need to do a protocol upgrade and want to make sure nothing is mixed up.</remarks>
        public void SetSingleDecode(bool singleDecode)
        {
            IsSingleDecode = singleDecode;
        }

        /// <summary>
        ///     Set the <see cref="Cumulator" /> function used by this decoder.
        /// </summary>
        public void SetCumulator(Cumulator cumulator)
        {
            Contract.Requires(cumulator != null);
            _cumulator = cumulator;
        }

        /// <summary>
        ///     Set the number of reads after whcih <see cref="IByteBuf.DiscardSomeReadBytes" /> are called to free up memory.
        ///     The default is <c>16</c>.
        /// </summary>
        /// <param name="discardAfterReads"></param>
        public void SetDiscardAfterReads(int discardAfterReads)
        {
            Contract.Requires(discardAfterReads > 0);
            _discardAfterReads = discardAfterReads;
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            var buf = InternalBuffer;
            var readable = buf.ReadableBytes;
            if (readable > 0)
            {
                var bytes = buf.ReadBytes(readable);
                buf.Release();
                context.FireChannelRead(bytes);
            }
            else
            {
                buf.Release();
            }
            _cumulation = null;
            _numReads = 0;
            context.FireChannelReadComplete();
            HandlerRemovedInternal(context);
        }

        protected virtual void HandlerRemovedInternal(IChannelHandlerContext context)
        {
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuf)
            {
                var output = RecyclableArrayList.Take();
                try
                {
                    var data = (IByteBuf) message;
                    _first = _cumulation == null;
                    if (_first)
                    {
                        _cumulation = data;
                    }
                    else
                    {
                        _cumulation = _cumulator(context.Allocator, _cumulation, data);
                    }
                    CallDecode(context, _cumulation, output);
                }
                catch (DecoderException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DecoderException(ex);
                }
                finally
                {
                    if (_cumulation != null && !_cumulation.IsReadable())
                    {
                        _numReads = 0;
                        _cumulation.Release();
                        _cumulation = null;
                    }
                    else if (++_numReads >= _discardAfterReads)
                    {
                        _numReads = 0;
                        DiscardSomeReadBytes();
                    }

                    var size = output.Count;
                    _decodeWasNull = size == 0;
                    FireChannelRead(context, output, size);
                    output.Return();
                }
            }
            else
            {
                // not a byte buffer? then we can't handle it. Forward it along 
                context.FireChannelRead(message);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            _numReads = 0;
            DiscardSomeReadBytes();
            if (_decodeWasNull)
            {
                _decodeWasNull = false;
                if (!context.Channel.Configuration.AutoRead)
                {
                    context.Read();
                }
            }
            context.FireChannelReadComplete();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            ChannelInputClosed(context, true);
        }

        private void ChannelInputClosed(IChannelHandlerContext context, bool callChannelInactive)
        {
            var output = RecyclableArrayList.Take();
            try
            {
                if (_cumulation != null)
                {
                    CallDecode(context, _cumulation, output);
                    DecodeLast(context, _cumulation, output);
                }
                else
                {
                    DecodeLast(context, Unpooled.Empty, output);
                }
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DecoderException(ex);
            }
            finally
            {
                try
                {
                    if (_cumulation != null)
                    {
                        _cumulation.Release();
                        _cumulation = null;
                    }
                    var size = output.Count;
                    FireChannelRead(context, output, size);

                    if (size > 0)
                    {
                        // Something was read, call FireChannelReadComplete()
                        context.FireChannelReadComplete();
                    }

                    if (callChannelInactive)
                    {
                        context.FireChannelInactive();
                    }
                }
                finally
                {
                    // recycle in all cases
                    output.Return();
                }
            }
        }

        protected void DiscardSomeReadBytes()
        {
            if (_cumulation != null && !_first && _cumulation.ReferenceCount == 1)
            {
                _cumulation.DiscardSomeReadBytes();
            }
        }

        private static void FireChannelRead(IChannelHandlerContext context, List<object> msgs, int numElements)
        {
            for (var i = 0; i < numElements; i++)
                context.FireChannelRead(msgs[i]);
        }

        protected void CallDecode(IChannelHandlerContext context, IByteBuf input, List<object> output)
        {
            try
            {
                while (input.IsReadable())
                {
                    var outSize = output.Count;
                    if (outSize > 0)
                    {
                        FireChannelRead(context, output, outSize);
                        output.Clear();

                        // Check if this handler was removed before continuing with decoding
                        // If it was removed, it is no longer safe to keep operating on the buffer
                        if (context.Removed)
                            break;

                        outSize = 0;
                    }

                    var oldInputLength = input.ReadableBytes;
                    Decode(context, input, output);

                    // Check if this handler was removed before continuing with decoding
                    // If it was removed, it is no longer safe to keep operating on the buffer
                    if (context.Removed)
                        break;

                    if (outSize == output.Count)
                    {
                        if (oldInputLength == input.ReadableBytes)
                        {
                            break;
                        }
                        continue;
                    }

                    if (oldInputLength == input.ReadableBytes)
                    {
                        throw new DecoderException($"{GetType()}.Decode() did not read anything but decoded a message.");
                    }

                    if (IsSingleDecode)
                        break;
                }
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DecoderException(ex);
            }
        }

        protected abstract void Decode(IChannelHandlerContext context, IByteBuf input, List<object> output);

        /// <summary>
        ///     Called one last time when the <see cref="IChannelHandlerContext" /> goes inactive, which means the
        ///     <see cref="IChannelHandler.ChannelInactive" /> was triggered.
        ///     By default this will jsut call <see cref="Decode" /> but sub-classes may override this for special cleanup
        ///     operations.
        /// </summary>
        protected virtual void DecodeLast(IChannelHandlerContext context, IByteBuf input, List<object> output)
        {
            if (input.IsReadable())
            {
                // Only call Decode if there is something left in the buffer to decode
                Decode(context, input, output);
            }
        }
    }
}