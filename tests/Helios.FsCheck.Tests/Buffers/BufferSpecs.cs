using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using Helios.Buffers;
using Xunit;

namespace Helios.FsCheck.Tests.Buffers
{
    public class BufferSpecs
    {
        public BufferSpecs()
        {
            Arb.Register(typeof (BufferGenerators));
        }

        [Theory]
        [InlineData(typeof(UnpooledByteBufAllocator))]
        public void Buffer_should_perform_consistent_reads_and_writes(Type allocatorType)
        {
            var allocator = (IByteBufAllocator)Activator.CreateInstance(allocatorType);

            var writeReadConsitency = Prop.ForAll<BufferOperations.IWrite[], BufferSize>((writes, size) =>
            {
                var buffer = allocator.Buffer(size.InitialSize, size.MaxSize);
                var expectedValues = writes.Select(x => x.UntypedData).ToList();
                var reads = writes.Select(x => x.ToRead());
                foreach (var write in writes)
                    write.Execute(buffer);

                var actualValues = new List<object>();
                foreach(var read in reads)
                    actualValues.Add(read.Execute(buffer));

                return expectedValues.SequenceEqual(actualValues, BufferOperations.Comparer).Label($"Expected: {string.Join(",", expectedValues)}; Got: {string.Join(",", actualValues)}");
            }).Label("Writes then reads in same order should produce original input");

            var writeIndexConsistency = Prop.ForAll<BufferOperations.IWrite[], BufferSize>((writes, size) =>
            {
                var writtenBytes = 0;
                var buffer = allocator.Buffer(size.InitialSize, size.MaxSize);
                foreach (var write in writes)
                    writtenBytes += write.Execute(buffer);

                return buffer.ReaderIndex == 0 && buffer.WriterIndex == writtenBytes;
            }).Label("Buffer's writer index should match total number of written bytes");

            writeReadConsitency.And(writeIndexConsistency).QuickCheckThrowOnFailure();
        }
    }
}
