using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Helios.Buffers;
using Helios.Channels;
using Helios.Concurrency;
using Helios.FsCheck.Tests.Buffers;

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
            var totalExpectedSize = writes.Sum(w => w.ReadableBytes);
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
    }
}
