using System.Text;
using Helios.Buffers;
using Xunit;

namespace Helios.Tests.Buffer
{
    public class BasicByteBufTests
    {
        [Fact]
        public void Should_pretty_print_buffer()
        {
            var buf = Unpooled.Buffer(10).WriteBoolean(true).WriteInt(4);
            var str = buf.ToString(Encoding.ASCII);
            Assert.NotNull(str);
        }
    }
}
