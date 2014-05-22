using Helios.Util.Collections;
using NUnit.Framework;

namespace Helios.Tests.Util.Collections
{
    [TestFixture]
    public class CircularBufferTests
    {
        [Test]
        public void CircularBuffer_should_expand()
        {
            var initialBuffer = new CircularBuffer<int>(10,30);
            initialBuffer.Capacity = 20; //should expand
            for(var i = 0; i < 20; i++)
                initialBuffer.Add(i);
            Assert.AreEqual(30, initialBuffer.Capacity);
            Assert.AreEqual(20, initialBuffer.Size);
        }
    }
}
