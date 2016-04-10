using System;
using System.Linq;
using System.Text;
using Helios.Util.Collections;
using NUnit.Framework;

namespace Helios.Tests.Util.Collections
{
    [TestFixture]
    public class CircularBufferTests
    {
        protected virtual ICircularBuffer<T> GetBuffer<T>(int capacity)
        {
            return new CircularBuffer<T>(capacity);
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
            var buffer = GetBuffer<byte>(byteArrayOne.Length);
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
            var buffer = GetBuffer<int>(5);
            foreach (var number in numbers)
            {
                buffer.Enqueue(number);
            }
            var resultNumbers = buffer.DequeueAll();
            Assert.IsTrue(finalNumbers.SequenceEqual(resultNumbers));
        }
    }
}
