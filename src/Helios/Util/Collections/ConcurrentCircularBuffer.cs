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
            Capacity = capacity;
            _size = 0;
            _head = 0;
            _tail = 0;
            _buffer = new T[Capacity];
        }

        public int MaxCapacity { get; private set; }
        public int Capacity { get;  set; }


        public int Size { get { return _size; } }
        public int Head { get { return _head % Capacity; } }
        public int Tail { get { return _tail % Capacity; } }

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
            throw new NotImplementedException();
        }

        public T ElementAt(int index)
        {
            throw new NotImplementedException();
        }

        public void SetElementAt(T element, int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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
            item = Dequeue();
            return item != null;
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
            throw new NotImplementedException();
        }

        public void DirectBufferWrite(T[] src, int srcLength)
        {
            throw new NotImplementedException();
        }

        public void DirectBufferWrite(T[] src, int srcIndex, int srcLength)
        {
            throw new NotImplementedException();
        }

        public void DirectBufferRead(T[] dest)
        {
            throw new NotImplementedException();
        }

        public void DirectBufferRead(T[] dest, int destLength)
        {
            throw new NotImplementedException();
        }

        public void DirectBufferRead(T[] dest, int destIndex, int destLength)
        {
            throw new NotImplementedException();
        }

        public void DirectBufferRead(int index, T[] dest, int destIndex, int destLength)
        {
            throw new NotImplementedException();
        }

        public void SetRange(int index, T[] values)
        {
            throw new NotImplementedException();
        }

        public void DirectBufferWrite(int index, T[] src, int srcIndex, int srcLength)
        {
            throw new NotImplementedException();
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
