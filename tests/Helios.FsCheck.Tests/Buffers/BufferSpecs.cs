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
        public static List<List<T>> ChunkOps<T>(List<T> source, int chunkSize)
        {
            var list = new List<List<T>>();
            for (var i = 0; i < source.Count; i = i + chunkSize)
            {
                list.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
            }
            return list;
        }

        public static string PrintByteArray(byte[] bytes)
        {
            return "byte[" + string.Join("|", bytes) + "]";
        }

        public static object PrintByteBufferItem(object item)
        {
            if (item is byte[])
            {
                var bytes = item as byte[];
                return PrintByteArray(bytes);
            }

            return item;
        }

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

            var interleavedBehavior = Prop.ForAll<BufferOperations.IWrite[], BufferSize>((writes, size) =>
            {
                var writeStages = ChunkOps(writes.ToList(), 4);
                var expectedValues = writes.Select(x => x.UntypedData).ToList();
                var buffer = allocator.Buffer(size.InitialSize, size.MaxSize);
                var actualValues = new List<object>();

                foreach (var stage in writeStages)
                {
                    var reads = stage.Select(x => x.ToRead());
                    foreach (var write in stage)
                        write.Execute(buffer);

                    foreach (var read in reads)
                        actualValues.Add(read.Execute(buffer));
                }

                return expectedValues.SequenceEqual(actualValues, BufferOperations.Comparer).Label($"Expected: {string.Join(",", expectedValues)}; Got: {string.Join(",", actualValues)}");
            });

             writeReadConsitency.And(writeIndexConsistency).And(interleavedBehavior).QuickCheckThrowOnFailure();
        }

        [Theory]
        [InlineData(typeof(UnpooledByteBufAllocator))]
        public void Buffer_should_be_able_to_change_endianness_without_data_corruption(Type allocatorType)
        {
            var allocator = (IByteBufAllocator)Activator.CreateInstance(allocatorType);
            var swappedWritesCanBeSwappedBack = Prop.ForAll<BufferOperations.IWrite[], BufferSize>((writes, size) =>
            {
                var buffer = allocator.Buffer(size.InitialSize, size.MaxSize).WithOrder(ByteOrder.LittleEndian);
                var expectedValues = writes.Select(x => x.UntypedData).ToList();
                var reads = writes.Select(x => x.ToRead());
                foreach (var write in writes)
                    write.Execute(buffer);

                var swappedBuffer = buffer.Copy().WithOrder(ByteOrder.BigEndian); // have to guarantee different endianness than before, and on a fresh copy
                Assert.NotSame(buffer, swappedBuffer);
                foreach (var write in writes)
                {
                    write.Execute(swappedBuffer);
                }

                var actualValues = new List<object>();
                var reversedValues = new List<object>();
                var swappedAgain = swappedBuffer.WithOrder(ByteOrder.LittleEndian);
                Assert.NotSame(buffer, swappedAgain); // should still be different copies
                foreach (var read in reads)
                {
                    actualValues.Add(read.Execute(buffer));
                    var reversedRead = read.Execute(swappedAgain);
                    reversedValues.Add(reversedRead); //is byte[] ? ((byte[])reversedRead).Reverse().ToArray() : reversedRead
                }

                return expectedValues.SequenceEqual(actualValues, BufferOperations.Comparer)
                .Label($"Expected: {string.Join(",", expectedValues)}; Got: {string.Join(",", actualValues)}")
                .And(() => actualValues.SequenceEqual(reversedValues, BufferOperations.Comparer)).Label($"Expected swapped values to match original [{string.Join(",", actualValues.Select(PrintByteBufferItem))}], but were [{string.Join(",", actualValues.Select(PrintByteBufferItem))}]");
            }).Label("Writes then reads against the reverse of the reverse should produce original input");

            swappedWritesCanBeSwappedBack.QuickCheckThrowOnFailure();
        }
    }
}
