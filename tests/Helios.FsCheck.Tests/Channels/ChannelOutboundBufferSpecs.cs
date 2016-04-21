using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Helios.Buffers;
using Helios.Channels;
using Helios.Concurrency;
using Helios.FsCheck.Tests.Buffers;
using Helios.Util;

namespace Helios.FsCheck.Tests.Channels
{
    /// <summary>
    /// Specs for verifying that the <see cref="ChannelOutboundBuffer"/> behaves as expected.
    /// </summary>
    public class ChannelOutboundBufferSpecs
    {
        public const int WriteLowWaterMark = 2048;
        public const int WriteHighWaterMark = WriteLowWaterMark*2;

        public ChannelOutboundBufferSpecs()
        {
            Arb.Register<BufferGenerators>();
        }

        [Property(QuietOnSuccess = true, MaxTest = 10000)]
        public Property ChannelOutboundBuffer_must_always_correctly_report_writability_and_PendingBytes(IByteBuf[] writes)
        {
            bool writeable = true;
            var buffer = new ChannelOutboundBuffer(WriteHighWaterMark, WriteLowWaterMark, () =>
            {
                writeable = !writeable; // toggle writeability
            });

            foreach (var msg in writes)
            {
                var initialSize = buffer.TotalPendingWriteBytes;
                var nextSize = initialSize + msg.ReadableBytes;
                var currentBytesUntilUnwriteable = buffer.BytesBeforeUnwritable;
                var currentBytesUntilWriteable = buffer.BytesBeforeWritable;
                var currentWriteability = writeable;
                long nextBytesUntilWriteable, nextBytesUntilUnwriteable;
                bool nextWritability;
                if (writeable)
                {
                    if (msg.ReadableBytes < currentBytesUntilUnwriteable) // should stay writable
                    {
                        nextWritability = writeable;
                        nextBytesUntilUnwriteable = currentBytesUntilUnwriteable - msg.ReadableBytes;
                        nextBytesUntilWriteable = 0; // already writable
                    }
                    else // should become unwriteable
                    {
                        nextWritability = !writeable;
                        nextBytesUntilWriteable = nextSize - WriteLowWaterMark;
                        nextBytesUntilUnwriteable = 0;
                    }
                }
                else //already unwriteable
                {
                    nextWritability = writeable;
                    nextBytesUntilWriteable = nextSize - WriteLowWaterMark;
                    nextBytesUntilUnwriteable = 0;
                }

                buffer.AddMessage(msg, msg.ReadableBytes, TaskCompletionSource.Void);

                var satisfiesGrowthPrediction = (nextWritability == writeable &&
                                                 nextBytesUntilUnwriteable == buffer.BytesBeforeUnwritable &&
                                                 nextBytesUntilWriteable == buffer.BytesBeforeWritable &&
                                                 nextSize == buffer.TotalPendingWriteBytes);
                if (!satisfiesGrowthPrediction)
                    return
                        false.Label(
                            $"Buffer failed to meet growth prediction for initial buffer size {initialSize} with next write of size {msg.ReadableBytes}." +
                            $"TotalPendingWriteBytes(ex={nextSize}, " +
                            $"ac={buffer.TotalPendingWriteBytes}), " +
                            $"Writability(ex={nextWritability}, ac={writeable}), " +
                            $"BytesUntilUnwriteable(ex={nextBytesUntilUnwriteable}, ac={buffer.BytesBeforeUnwritable}), " +
                            $"BytesUntilWriteable(ex={nextBytesUntilWriteable}, ac={buffer.BytesBeforeWritable})");
            }

            return true.ToProperty();
        }

        [Property(QuietOnSuccess = true, StartSize = 2, MaxTest = 5000)]
        public Property ChannelOutboundBuffer_must_always_process_pending_writes_in_FIFO_order(IByteBuf[] writes)
        {
            if (writes.Length == 0) // skip any zero-length results
                return true.ToProperty(); 
            bool writeable = true;
            var buffer = new ChannelOutboundBuffer(WriteHighWaterMark, WriteLowWaterMark, () =>
            {
                writeable = !writeable; // toggle writeability
            });

            foreach (var message in writes)
            {
                buffer.AddMessage(message, message.ReadableBytes, TaskCompletionSource.Void);
            }
            buffer.AddFlush(); // have to flush in order to put messages into a readable state

            var comparer = BufferOperations.Comparer;
            var iterator = writes.GetEnumerator();
            iterator.MoveNext();
            var position = 0;
            do
            {
                var read = buffer.Current as IByteBuf;
                var match = comparer.Equals(read, iterator.Current);
                if (!match)
                    return
                        false.Label(
                            $"ChannelOutboundBuffer item at position {position} did not match the expected value.");
                position++;
            } while (buffer.Remove() && iterator.MoveNext());

            return true.ToProperty();
        }

        [Property(QuietOnSuccess = true, MaxTest = 10000)]
        public Property ChannelOutboundBuffer_must_complete_all_promises_successfully_on_read_regardless_of_flush_order(IByteBuf[] writes)
        {
            var tasks = new List<Task>();
            bool writeable = true;
            var buffer = new ChannelOutboundBuffer(WriteHighWaterMark, WriteLowWaterMark, () =>
            {
                writeable = !writeable; // toggle writeability
            });

            // write + flush loop
            foreach (var message in writes)
            {
                var tcs = new TaskCompletionSource();
                tasks.Add(tcs.Task);
                buffer.AddMessage(message, message.ReadableBytes, tcs);
                if(ThreadLocalRandom.Current.Next(0,1) == 0) // just doing this to guarantee we don't screw up the linked list
                    buffer.AddFlush();
            }
            buffer.AddFlush(); // add a final flush in case RNG hasn't let us do one in a while

            // sanity check
            if (buffer.Count != writes.Length)
                return
                    false.Label(
                        $"Expected buffer to have {writes.Length} writes waiting for processing; found {buffer.Count} instead.");

            var completion = Task.WhenAll(tasks);
            if (!tasks.TrueForAll(x => x.IsCompleted == false))
                return
                    false.Label(
                        "None of the messages have been processed, but still found Tasks reporting successful completion");

            do
            {
                // process all of the messages
            } while (buffer.Remove());

            return completion.IsCompleted.Label("Expected all underlying tasks to be completed.");
        }

        [Property(QuietOnSuccess = true)]
        public Property ChannelOutboundBuffer_must_dump_all_flushed_messages_upon_failure(IByteBuf[] writes)
        {
            var tasks = new List<Task>();
            bool writeable = true;
            var buffer = new ChannelOutboundBuffer(WriteHighWaterMark, WriteLowWaterMark, () =>
            {
                writeable = !writeable; // toggle writeability
            });

            // write
            foreach (var message in writes)
            {
                var tcs = new TaskCompletionSource();
                tasks.Add(tcs.Task);
                buffer.AddMessage(message, message.ReadableBytes, tcs);
            }
            buffer.AddFlush(); // flush em

            var completion = Task.WhenAll(tasks);

            // err
            var exception = new ApplicationException("TEST");
            buffer.FailFlushed(exception, true);

            return (completion.IsFaulted && buffer.IsWritable && tasks.All(x => x.IsFaulted && x.Exception.InnerExceptions.Contains(exception)))
                .When(writes.Length > 0)
                .Label("Expected all flushed messages to be faulted with the same exception upon FailFlush, and for buffer to be writable.");
        }
    }
}
