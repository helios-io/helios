using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Helios.Util.Collections
{
    /// <summary>
    /// Concurrent circular buffer implementation, synchronized using a monitor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentCircularBuffer<T> : ICircularBuffer<T>
    {
        //Non-expanding buffer
        public ConcurrentCircularBuffer(int capacity) : this(capacity, capacity) { }

        public ConcurrentCircularBuffer(int capacity, int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            InternalCapacity = capacity;
            _size = 0;
            _head = 0;
            _tail = 0;
            _buffer = new T[Capacity];
        }

        /// <summary>
        /// The size of the buffer
        /// </summary>
        protected int InternalCapacity;

        public int MaxCapacity { get; private set; }
        public int Capacity
        {
            get { return InternalCapacity; }
            set
            {
                lock (m_lockObject)
                {
                    if (value == InternalCapacity)
                        return;

                    if (value > InternalCapacity && InternalCapacity < MaxCapacity) //expand
                    {
                        var newCapacity = CalculateNewCapacity(value);
                        Expand(newCapacity);
                        InternalCapacity = newCapacity;

                    }
                    else if (value < InternalCapacity) //shrink
                    {
                        Shrink(value);
                        InternalCapacity = value;
                    }
                }
            }
        }


        public int Size { get { return _size; } }
        public int Head { get { return _head % Capacity; } }
        public int Tail { get { return _tail % Capacity; } }

        /// <summary>
        /// Grow the capacity by a power of two, so we aren't having to constantly expand it over and over again
        /// during periods of sustained writes.
        /// </summary>
        /// <param name="minNewCapacity">The minimum additional space we need to accommodate</param>
        protected int CalculateNewCapacity(int minNewCapacity)
        {
            var maxCapacity = MaxCapacity;
            var threshold = 1048576; // 1 MiB page if sizeof(T) == 1 byte. Most T will be bigger than 1 byte.
            var newCapacity = 0;
            if (minNewCapacity == threshold)
            {
                return threshold;
            }

            // If over threshold, do not double but just increase by threshold.
            if (minNewCapacity > threshold)
            {
                newCapacity = minNewCapacity / threshold * threshold;
                if (newCapacity > maxCapacity - threshold)
                {
                    newCapacity = maxCapacity;
                }
                else
                {
                    newCapacity += threshold;
                }
                return newCapacity;
            }

            // Not over threshold. Double up to 1MB, starting from 64.
            newCapacity = 64;
            while (newCapacity < minNewCapacity)
            {
                newCapacity <<= 1;
            }

            return Math.Min(newCapacity, maxCapacity);
        }

        public virtual void Shrink(int newSize)
        {
            var previousSize = _size;
            var newBuffer = new T[newSize];
            CopyTo(newBuffer, 0, newSize);
            _buffer = newBuffer;
            _head = 0;
            _tail = previousSize;
            _size = newSize;
        }

        /// <summary>
        /// Expands the circular buffer to accommodate additional space
        /// </summary>
        public virtual void Expand(int newSize)
        {
            var previousSize = _size; //number of elements in the array
            var newBuffer = new T[newSize];
            CopyTo(newBuffer);
            _buffer = newBuffer;
            _head = 0;
            _tail = previousSize;
        }

        public void SetHead(int position)
        {
            IncrementHead(position - Head);
        }

        public void SetTail(int position)
        {
            IncrementTail(position - Head);
        }

        public void IncrementHead(int increment)
        {
            lock (m_lockObject)
            {
                _head += increment;
                _size -= increment;
                if (_size < 0)
                    _size = 0;
            }
           
        }

        public void IncrementTail(int increment)
        {
            lock (m_lockObject)
            {
                _tail += increment;
                _size += increment;
                if (_size > Capacity)
                    _size = Capacity;
            }
        }


        #region Internal members

        private readonly object m_lockObject = new object();

        private int _size;
        private int _head;
        private int _tail;

        /// <summary>
        /// The buffer itself
        /// </summary>
        private T[] _buffer;

        #endregion

        public T Peek()
        {
            if (Size == 0)
                return default(T);
            return _buffer[(_head % Capacity)];
        }

        public void Enqueue(T obj)
        {
            if (_size + 1 > Capacity)
            {
                Capacity += 1; //expand by 1 (or no-op if expansion isn't supported)
            }

            lock (m_lockObject)
            {
                _buffer[_tail % Capacity] = obj;
                _tail++;
                if (_size < Capacity)
                    _size++;
            }
        }

        public void Enqueue(T[] objs)
        {
            //Expand
            if (_size + objs.Length >= Capacity)
                Capacity += objs.Length;

            lock (m_lockObject)
            {
                foreach (var item in objs)
                {
                    _buffer[_tail % Capacity] = item;
                    _tail++;
                    if (_size < Capacity)
                        _size++;
                }
            }
        }

        public T Dequeue()
        {

            T item;

            lock (m_lockObject)
            {
                if (Size == 0) //Don't move the read head if it's empty
                    return default(T);

                item = _buffer[_head % Capacity];
                _head++;
                _size--;
            }

            return item;
        }

        public void Skip(int length)
        {
            lock (m_lockObject)
            {
                if (_size >= length)
                {
                    _head += length;
                    _size -= length;
                }
            }
        }

        public IEnumerable<T> Dequeue(int count)
        {

            T[] returnItems;

            lock (m_lockObject)
            {
                var availabileItems = Math.Min(count, Size);
                returnItems = new T[availabileItems];

                if (returnItems.Length == 0)
                    return returnItems;

                for (var i = 0; i < availabileItems; i++)
                {
                    returnItems[i] = _buffer[_head % Capacity];
                    _head++;
                    _size--;
                }
            }


            return returnItems;
        }

        public IEnumerable<T> DequeueAll()
        {
            T[] resultArray;

            lock (m_lockObject)
            {
                resultArray = new T[_size];
                var availableItems = _size;
                for (var i = 0; i < availableItems; i++)
                {
                    resultArray[i] = _buffer[_head % Capacity];
                    _head++;
                    _size--;
                }
            }

            return resultArray;
        }

        public bool IsElementAt(int index)
        {
            return ((_head + index) % Capacity) <= Size;
        }

        public T ElementAt(int index)
        {
            lock (m_lockObject)
            {
                return _buffer[(_head + index) % Capacity];
            }
        }

        public void SetElementAt(T element, int index)
        {
            lock (m_lockObject)
            {
                _buffer[(_head + index) % Capacity] = element;
            }
        }

        public T this[int index]
        {
            get { return ElementAt(index); }
            set { SetElementAt(value, index); }
        }

        public void Add(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            lock (m_lockObject)
            {
                _size = 0;
                _head = 0;
                _tail = 0;
            }
        }

        public bool Contains(T item)
        {
            lock (m_lockObject)
            {
                return _buffer.Any(x => x.GetHashCode() == item.GetHashCode());
            }
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, Size);
        }

        public bool Remove(T item)
        {
            return false;
        }

        public bool TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            item = default(T);
            if (Size == 0) return false;
            item = Dequeue();
            return true;
        }

        public T[] ToArray()
        {
            var bufferCopy = new T[_size];
            CopyTo(bufferCopy, 0, bufferCopy.Length);
            return bufferCopy;
        }

        public void CopyTo(T[] array, int index, int count)
        {
            //Make a copy of the internal buffer and pointers
            var internalCopy = new T[Capacity];
            var internalHead = 0;
            var internalSize = 0;

            lock (m_lockObject)
            {
                _buffer.CopyTo(internalCopy, 0);
                internalHead = _head;
                internalSize = _size;
            }

            /*
             * Now we're thread safe. Have a separate copy of each to work with.
             */

            if (count > internalSize) //The maximum value of count is Size
                count = internalSize;

            //COPY
            for (var i = 0; i < count; i++, internalHead++, index++)
            {
                array[index] = internalCopy[internalHead % Capacity];
            }
        }

        public void DirectBufferWrite(T[] src)
        {
            DirectBufferWrite(src, 0, src.Length);
        }

        public void DirectBufferWrite(T[] src, int srcLength)
        {
            DirectBufferWrite(src, 0, srcLength);
        }

        public void DirectBufferWrite(T[] src, int srcIndex, int srcLength)
        {
            DirectBufferWrite(0, src, srcIndex, srcLength);
            _tail += srcLength;
            _size += srcLength;
            if (_size > Capacity) _size = Capacity; //wrapped around
        }

        public void DirectBufferRead(T[] dest)
        {
            DirectBufferRead(dest, 0, dest.Length);
        }

        public void DirectBufferRead(T[] dest, int destLength)
        {
            DirectBufferRead(dest, 0, destLength);
        }

        public void DirectBufferRead(T[] dest, int destIndex, int destLength)
        {
            DirectBufferRead(0, dest, destIndex, destLength);
            _head += destLength;
            _size -= destLength;
        }

        public void DirectBufferRead(int index, T[] dest, int destIndex, int destLength)
        {
            if (destIndex + destLength > dest.Length) throw new ArgumentOutOfRangeException(string.Format("destIndex: {0}, destLength: {1} - expected destIndex + srcLength to be less than dest.Length ({2})", destIndex, destLength, dest.Length));
            if (_size < destLength) throw new ArgumentOutOfRangeException(string.Format("destLength: {0}, expected destLength to be less than or equal to the size of the buffer ({1})", destLength, Size));

            lock (m_lockObject)
            {
                var adjustedIndex = (_head + index) % Capacity;

                //check to see if we have enough contiguous space from the current index to the end of the buffer
                if (adjustedIndex + destLength < Capacity) //We have enough room to perform a contiguous read
                {
                    Array.Copy(_buffer, adjustedIndex, dest, destIndex, destLength);
                }
                else //we have to wrap around and perform the read in two steps
                {
                    var firstCommitLength = Capacity - adjustedIndex;
                    var secondCommitLength = destLength - firstCommitLength;
                    Array.Copy(_buffer, adjustedIndex, dest, destIndex, firstCommitLength);
                    Array.Copy(_buffer, 0, dest, destIndex + firstCommitLength, secondCommitLength);
                }
            }
        }

        public void SetRange(int index, T[] values)
        {
            DirectBufferWrite(index, values, 0, values.Length);
        }

        public void DirectBufferWrite(int index, T[] src, int srcIndex, int srcLength)
        {
            if (srcIndex + srcLength > src.Length) throw new ArgumentOutOfRangeException(string.Format("srcIndex: {0}, srcLength: {1} - expected srcIndex + srcLength to be less than src.Length ({2}", srcIndex, srcLength, src.Length));
            if (_size + srcLength >= Capacity)
                Capacity += srcLength; //expand

            lock (m_lockObject)
            {
                var adjustedIndex = (_tail + index) % Capacity;

                //check to see if we have enough contiguous space from the index to the end of the buffer
                if (adjustedIndex + srcLength < Capacity) //we can fit this inside one contiguous write
                {
                    Array.Copy(src, srcIndex, _buffer, adjustedIndex, srcLength);
                }
                else //have to commit this write into two phases and wrap-around
                {
                    var firstCommitLength = Capacity - adjustedIndex;
                    var secondCommitLength = srcLength - firstCommitLength;
                    Array.Copy(src, srcIndex, _buffer, adjustedIndex, firstCommitLength);
                    Array.Copy(src, srcIndex + firstCommitLength, _buffer, 0, secondCommitLength);
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {

            return ((IEnumerable<T>)ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return Size; } }
        public bool IsReadOnly { get { return false; } }
        public object SyncRoot { get { return m_lockObject; } }

        public bool IsSynchronized { get { return true; } }
    }
}
