using System.Linq;
using System.Text;
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

        /// <summary>
        /// If a circular buffer is defined with a fixed maximum capacity, it should
        /// simply overwrite the old elements even if they haven't been dequed
        /// </summary>
        [Test]
        public void CircularBuffer_should_not_expand()
        {
            var byteArrayOne = Encoding.Unicode.GetBytes("ONE STRING");
            var byteArrayTwo = Encoding.Unicode.GetBytes("TWO STRING");
            var buffer = new CircularBuffer<byte>(byteArrayOne.Length);
            buffer.Enqueue(byteArrayOne);
            Assert.AreEqual(buffer.Size, buffer.Capacity);
            buffer.Enqueue(byteArrayTwo);
            Assert.AreEqual(buffer.Size, buffer.Capacity);
            var availableBytes = buffer.DequeueAll().ToArray();
            Assert.IsFalse(byteArrayOne.SequenceEqual(availableBytes));
            Assert.IsTrue(byteArrayTwo.SequenceEqual(availableBytes));
        }

        [Test]
        public void CircularBuffer_should_keep_adding_elements_after_Max_Capacity()
        {
            var numbers = Enumerable.Range(1, 10000).ToList();
            var finalNumbers = Enumerable.Range(9801, 200).ToList();
            var buffer = new CircularBuffer<int>(5, 200);
            foreach (var number in numbers)
            {
                buffer.Add(number);
            }
            var resultNumbers = buffer.DequeueAll();
            Assert.IsTrue(finalNumbers.SequenceEqual(resultNumbers));
        }
    }
}
