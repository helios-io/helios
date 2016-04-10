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
        public CircularBuffer(T[] initialArray) : this(initialArray, initialArray.Length)
        {
        }

        public CircularBuffer(T[] initialArray, int capacity)
        {
            Capacity = capacity;
            _head = 0;
            _tail = 0;
            Buffer = initialArray;
        }

        public CircularBuffer(int capacity) : this(new T[capacity], capacity)
        {

        }

        /// <summary>
        /// Front of the buffer
        /// </summary>
        private int _head;

        /// <summary>
        /// Back of the buffer
        /// </summary>
        private int _tail;

        /// <summary>
        /// The buffer itself
        /// </summary>
        protected T[] Buffer;

        public int Capacity { get; }

        public int Size => (_tail - _head + Capacity)%Capacity;

        public int Head => _head % Capacity;
        public int Tail => _tail % Capacity;

        public virtual T Peek()
        {
            if (Size == 0)
                return default(T);
            return Buffer[(_head % Capacity)];
        }

        /// <summary>
        /// Checks an index relative to the <see cref="_head"/> to see if there's a set element there
        /// </summary>
        public bool IsElementAt(int index)
        {
            return ((_head + index) % Capacity) <= Size;
        }

        public T ElementAt(int index)
        {
            return Buffer[(_head + index) % Capacity];
        }

        /// <summary>
        /// Sets an element at the specified position relative to <see cref="_head"/>
        /// WITHOUT MODIFYING <see cref="_tail"/>
        /// </summary>
        /// <param name="element">The element we want to add at the specified <see cref="index"/></param>
        /// <param name="index">The index relative to the front of the buffer where we want to add <see cref="element"/></param>
        public void SetElementAt(T element, int index)
        {
            Buffer[(_head + index) % Capacity] = element;
        }

        public T this[int index]
        {
            get { return ElementAt(index); }
            set { SetElementAt(value, index); }
        }

        public virtual void Enqueue(T obj)
        {
            Buffer[_tail % Capacity] = obj;
            _tail++;
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
            if (Size == 0)
                return default(T);

            var item = Buffer[_head % Capacity];
            _head++;
            return item;
        }

        public virtual IEnumerable<T> Dequeue(int count)
        {
            var availabileItems = Math.Min(count, Size);
            var returnItems = new List<T>(availabileItems);
            for (var i = 0; i < availabileItems; i++, _head++)
            {
                returnItems.Add(Buffer[_head % Capacity]);
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

            var bufferBegin = Head;
            for (var i = 0; i < count; i++, bufferBegin++, index++)
            {
                if (bufferBegin == Capacity)
                    bufferBegin = 0; //Jump to the front of the buffer
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
    }
}
