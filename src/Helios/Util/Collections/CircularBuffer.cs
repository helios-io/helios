using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Helios.Util.Collections
{
    /// <summary>
    /// Base class for working with circular buffers
    /// </summary>
    /// <typeparam name="T">The type being stored in the circular buffer</typeparam>
    public class CircularBuffer<T> : ICircularBuffer<T>
    {
        public CircularBuffer(int capacity)
        {
            _head = 0;
            _tail = 0;
            Buffer = new T[capacity];
        }

        /// <summary>
        /// Front of the buffer
        /// </summary>
        private int _head;

        /// <summary>
        /// FOR TESTING PURPOSES ONLY
        /// </summary>
        internal int Head => _head;

        /// <summary>
        /// Back of the buffer
        /// </summary>
        private int _tail;

        /// <summary>
        /// FOR TESTING PURPOSES ONLY
        /// </summary>
        internal int Tail => _tail;

        /// <summary>
        /// The buffer itself
        /// </summary>
        protected T[] Buffer;

        public int Capacity => Buffer.Length;

        // We use an N+1 trick here to make sure we can distinguish full queues from empty ones
        public int Size => _full ? Capacity : (_tail - _head + Capacity) % Capacity;

        private bool _full = false;

        public virtual T Peek()
        {
            return Buffer[_head];
        }

        public virtual void Enqueue(T obj)
        {
            _full = _full || _tail + 1 == Capacity; // leave FULL flag on
            Buffer[_tail] = obj;
            _tail = (_tail + 1) % Capacity;
        }

        public void Enqueue(T[] objs)
        {
            foreach (var item in objs)
            {
                Enqueue(item);
            }
        }

        public virtual T Dequeue()
        {
            _full = false; // full is always false as soon as we dequeue
            var item = Buffer[_head];
            _head = (_head + 1) % Capacity;
            return item;
        }

        public virtual IEnumerable<T> Dequeue(int count)
        {
            var availabileItems = Math.Min(count, Size);
            var returnItems = new List<T>(availabileItems);
            for (var i = 0; i < availabileItems; i++, _head++)
            {
                returnItems.Add(Dequeue());
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

        public virtual void Clear()
        {
            _head = 0;
            _tail = 0;
        }

        public bool Contains(T item)
        {
            return Buffer.Any(x => x.GetHashCode() == item.GetHashCode());
        }

        /// <summary>
        /// Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        /// Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
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
            var bufferCopy = new T[Size];
            CopyTo(bufferCopy, 0, bufferCopy.Length);
            return bufferCopy;
        }

        /// <summary>
        /// Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        public void CopyTo(T[] array, int index, int count)
        {
            if (count > Size) //The maximum value of count is Size
                count = Size;

            var bufferBegin = _head;
            for (var i = 0; i < count; i++, bufferBegin = (bufferBegin+1) % Capacity, index++)
            {
                array[index] = Buffer[bufferBegin];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        public int Count => Size;

        public bool IsReadOnly { get; private set; }
        public virtual object SyncRoot { get; private set; }

        public virtual bool IsSynchronized => false;

        public override string ToString()
        {
            return $"CircularBuffer<{GetType()}>(Capacity = {Capacity})";
        }
    }
}
