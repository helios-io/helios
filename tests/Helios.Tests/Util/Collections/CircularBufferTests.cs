// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Linq;
using System.Text;
using Helios.Util.Collections;
using Xunit;

namespace Helios.Tests.Util.Collections
{
    public class CircularBufferTests
    {
        protected virtual ICircularBuffer<T> GetBuffer<T>(int capacity)
        {
            return new CircularBuffer<T>(capacity);
        }


        /// <summary>
        ///     If a circular buffer is defined with a fixed maximum capacity, it should
        ///     simply overwrite the old elements even if they haven't been dequed
        /// </summary>
        [Fact]
        public void CircularBuffer_should_not_expand()
        {
            var byteArrayOne = Encoding.Unicode.GetBytes("ONE STRING");
            var byteArrayTwo = Encoding.Unicode.GetBytes("TWO STRING");
            var buffer = GetBuffer<byte>(byteArrayOne.Length);
            buffer.Enqueue(byteArrayOne);
            Assert.Equal(buffer.Size, buffer.Capacity);
            buffer.Enqueue(byteArrayTwo);
            Assert.Equal(buffer.Size, buffer.Capacity);
            var availableBytes = buffer.DequeueAll().ToArray();
            Assert.False(byteArrayOne.SequenceEqual(availableBytes));
            Assert.True(byteArrayTwo.SequenceEqual(availableBytes));
        }
    }
}

