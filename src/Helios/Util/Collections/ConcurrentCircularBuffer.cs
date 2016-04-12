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
        public ConcurrentCircularBuffer(int capacity)
        {
            _head = 0;
            _tail = 0;
            _buffer = new T[capacity];
        }

        public int Capacity => _buffer.Length;

        private bool _full;


        public int Size
        {
            get
            {
                lock (m_lockObject)
                {
                    return SizeUnsafe;
                }
            }
        }

        private int SizeUnsafe => _full ? Capacity : (_tail - _head + Capacity)%Capacity;

        /// <summary>
        /// FOR TESTING PURPOSES ONLY
        /// </summary>
        internal int Head => _head;

        /// <summary>
        /// FOR TESTING PURPOSES ONLY
        /// </summary>
        internal int Tail => _tail;

        #region Internal members

        private readonly object m_lockObject = new object();

        private int _head;
        private int _tail;

        /// <summary>
        /// The buffer itself
        /// </summary>
        private T[] _buffer;

        #endregion

        public T Peek()
        {
            lock (m_lockObject)
            {
                return _buffer[_head];
            }

        }

        public void Enqueue(T obj)
        {
            lock (m_lockObject)
            {
                UnsafeEnqueue(obj);
            }
        }

        private void UnsafeEnqueue(T obj)
        {
            _full = _full || _tail + 1 == Capacity; // leave FULL flag on
            _buffer[_tail] = obj;
            _tail = (_tail + 1) % Capacity;
        }

        public void Enqueue(T[] objs)
        {
            //Expand
            lock (m_lockObject)
            {
                foreach (var item in objs)
                {
                    UnsafeEnqueue(item);
                }
            }
        }

        public T Dequeue()
        {
            lock (m_lockObject)
            {
                return UnsafeDequeue();
            }
        }

        private T UnsafeDequeue()
        {
            _full = false; // full is always false as soon as we dequeue
            var item = _buffer[_head];
            _head = (_head + 1) % Capacity;
            return item;
        }

        public IEnumerable<T> Dequeue(int count)
        {

            IList<T> returnItems;

            lock (m_lockObject)
            {
                var availabileItems = Math.Min(count, SizeUnsafe);
                returnItems = new List<T>(availabileItems);

                if (returnItems.Count == 0)
                    return returnItems;

                for (var i = 0; i < availabileItems; i++)
                {
                    returnItems.Add(UnsafeDequeue());
                }
            }

            return returnItems;
        }

        public IEnumerable<T> DequeueAll()
        {
            return Dequeue(Size);
        }
      
        public void Add(T item)
        {
            Enqueue(item);
        }

        public void Clear()
        {
            lock (m_lockObject)
            {
                _head = 0;
                _tail = 0;
                _full = false;
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
            lock (m_lockObject)
            {
                var bufferCopy = new T[SizeUnsafe];
                CopyToUnsafe(bufferCopy, 0, bufferCopy.Length);
                return bufferCopy;
            }
        }

        private void CopyToUnsafe(T[] array, int index, int count)
        {
            if (count > SizeUnsafe) //The maximum value of count is Size
                count = SizeUnsafe;

            var bufferBegin = _head;
            for (var i = 0; i < count; i++, bufferBegin = (bufferBegin + 1) % Capacity, index++)
            {
                array[index] = _buffer[bufferBegin];
            }
        }

        public void CopyTo(T[] array, int index, int count)
        {
            lock (m_lockObject)
            {
                CopyToUnsafe(array, index, count);
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
