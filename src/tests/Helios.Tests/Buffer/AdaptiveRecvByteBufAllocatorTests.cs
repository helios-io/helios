using Helios.Buffers;
using NUnit.Framework;

namespace Helios.Tests.Buffer
{
    [TestFixture]
    public class AdaptiveRecvByteBufAllocatorTests
    {
        [Test]
        public void Should_initialize_default()
        {
            var def = AdaptiveRecvByteBufAllocator.Default;
            var handle = def.NewHandle();
            Assert.AreEqual(1024, handle.Guess());
        }

        [Test]
        public void Should_initialize_new()
        {
            var min = 100;
            var max = 10000;
            var init = 1024;

            var def = new AdaptiveRecvByteBufAllocator(min, init, max);
            var handle = def.NewHandle();

            Assert.AreEqual(init, handle.Guess());
        }
    }
}