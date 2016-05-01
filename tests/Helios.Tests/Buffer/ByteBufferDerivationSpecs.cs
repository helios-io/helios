using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Buffers;
using Xunit;

namespace Helios.Tests.Buffer
{
    public class ByteBufferDerivationSpecs
    {
        [Fact]
        public void Swap_in_reverse_should_be_original()
        {
            var buf = Unpooled.Buffer(8).SetIndex(1, 7);
            var swapped = buf.WithOrder(ByteOrder.BigEndian);

            Assert.IsType<SwappedByteBuffer>(swapped);
            Assert.Null(swapped.Unwrap());
            Assert.Same(buf, swapped.WithOrder(ByteOrder.LittleEndian));
            Assert.Same(swapped, swapped.WithOrder(ByteOrder.BigEndian));
            buf.SetIndex(2, 6);
            Assert.Equal(swapped.ReaderIndex, 2);
            Assert.Equal(swapped.WriterIndex, 6);
        }
    }
}
