using System;
using System.Collections.Generic;
using Helios.Channel;

namespace Helios.Buffers
{
    /// <summary>
    /// The <see cref="IRecvByteBufAllocator"/> implementation that automatically increases and
    /// decreases the predicted buffer size on feed back.
    /// It gradually increases the expected number of readable bytes if the previous
    /// read fully filled the allocated buffer.  It gradually decreases the expected
    /// number of readable bytes if the read operation was not able to fill a certain
    /// amount of the allocated buffer two times consecutively.  Otherwise, it keeps
    /// returning the same prediction.
    /// </summary>
    public class AdaptiveRecvByteBufAllocator : IRecvByteBufAllocator
    {
        // ReSharper disable InconsistentNaming
        private const int DEFAULT_MINIMUM = 64;
        private const int DEFAULT_INITIAL = 1024;
        private const int DEFAULT_MAXIMUM = 65536;
        private const int INDEX_INCREMENT = 4;
        private const int INDEX_DECREMENT = 1;

        private static int[] SIZE_TABLE;
        private readonly int _minIndex;
        private readonly int _maxIndex;
        private readonly int _initial;

        /// <summary>
        /// Creates a new predictor with the default parameters.  With the default
        /// parameters, the expected buffer size starts from <c>1024</c>, does not
        /// go down below <c>64</c>, and does not go up above <c>65536</c>.
        /// </summary>
        private AdaptiveRecvByteBufAllocator()
            : this(DEFAULT_MINIMUM, DEFAULT_INITIAL, DEFAULT_MAXIMUM)
        {
            
        }

        /// <summary>
        /// Creates a new predictor with the specified parameters.
        /// </summary>
        /// <param name="minimum">The inclusive lower bound of the expected buffer size</param>
        /// <param name="initial">The initial buffer size when no feed back was received</param>
        /// <param name="maximum">The inclusive upper bound of the expected buffer size</param>
        public AdaptiveRecvByteBufAllocator(int minimum, int initial, int maximum)
        {
            if (minimum <= 0)
                throw new ArgumentOutOfRangeException("minimum", "Cannot be negative value");
            if (initial < minimum)
                throw new ArgumentOutOfRangeException("initial", "Cannot be less than minimum");
            if (maximum < initial)
                throw new ArgumentOutOfRangeException("maximum", "Cannot be less than initial");

            var sizeTable = new List<int>();
            for (var i = 16; i < 512; i += 16)
            {
                sizeTable.Add(i);
            }

            for (var i = 512; i > 0; i <<= 1)
            {
                sizeTable.Add(i);
            }

            SIZE_TABLE = new int[sizeTable.Count];
            for (var i = 0; i < SIZE_TABLE.Length; i++)
            {
                SIZE_TABLE[i] = sizeTable[i];
            }

            var minIndex = GetSizeTableIndex(minimum);
            if (SIZE_TABLE[minIndex] < minimum)
            {
                _minIndex = minIndex + 1;
            }
            else
            {
                _minIndex = minIndex;
            }

            var maxIndex = GetSizeTableIndex(maximum);
            if (SIZE_TABLE[maxIndex] > maximum)
            {
                _maxIndex = maxIndex - 1;
            }
            else
            {
                _maxIndex = maxIndex;
            }

            _initial = initial;
        }

        public static AdaptiveRecvByteBufAllocator Default = new AdaptiveRecvByteBufAllocator();

        private static int GetSizeTableIndex(int size)
        {
            for (int low = 0, high = SIZE_TABLE.Length - 1;;)
            {
                if (high < low)
                {
                    return low;
                }

                if (high == low)
                {
                    return high;
                }

                var mid = (int)(((uint)low + high) >> 1);
                var a = SIZE_TABLE[mid];
                var b = SIZE_TABLE[mid + 1];
                if (size > b)
                {
                    low = mid + 1;
                }
                else if (size < a)
                {
                    high = mid - 1;
                }
                else if (size == a)
                {
                    return mid;
                }
                else
                {
                    return mid + 1;
                }
            }
        }

        private class HandleImpl : IRecvByteBufAllocatorHandle
        {
            private readonly int _minIndex;
            private readonly int _maxIndex;
            private int _index;
            private int _nextReceiveBufferSize;
            private bool _decreaseNow;

            public HandleImpl(int minIndex, int maxIndex, int initial)
            {
                _minIndex = minIndex;
                _maxIndex = maxIndex;

                _index = GetSizeTableIndex(initial);
                _nextReceiveBufferSize = SIZE_TABLE[_index];
            }

            public IByteBuf Allocate(IByteBufAllocator allocator)
            {
                return allocator.Buffer(_nextReceiveBufferSize);
            }

            public int Guess()
            {
                return _nextReceiveBufferSize;
            }

            public void Record(int actualReadBytes)
            {
                if (actualReadBytes <= SIZE_TABLE[Math.Max(0, _index - INDEX_DECREMENT - 1)])
                {
                    if (_decreaseNow)
                    {
                        _index = Math.Max(_index - INDEX_DECREMENT, _minIndex);
                        _nextReceiveBufferSize = SIZE_TABLE[_index];
                        _decreaseNow = false;
                    }
                    else
                    {
                        _decreaseNow = true;
                    }
                }
                else if (actualReadBytes >= _nextReceiveBufferSize)
                {
                    _index = Math.Min(_index + INDEX_INCREMENT, _maxIndex);
                    _nextReceiveBufferSize = SIZE_TABLE[_index];
                    _decreaseNow = false;
                }
            }
        }

        public IRecvByteBufAllocatorHandle NewHandle()
        {
            return new HandleImpl(_minIndex, _maxIndex, _initial);
        }
    }
}