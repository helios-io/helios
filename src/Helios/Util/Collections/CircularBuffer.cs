using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Helios.Util.Collections
{
    /// <summary>
    /// Base class for working with circular buffers
    /// </summary>
    /// <typeparam name="T">The type being stored in the circular buffer</typeparam>
    public class CircularBuffer<T> : ICircularBuffer<T> where T : class
    {
        protected internal CircularBuffer(int capacity)
        {
            InternalCapacity = capacity;
            InternalSize = 0;
            Head = 0;
            Tail = 0;
            Buffer = new T[Capacity];
        }

        /// <summary>
        /// The maximum size of the buffer
        /// </summary>
        protected int InternalCapacity;

        /// <summary>
        /// The current fill level of the buffer
        /// </summary>
        protected int InternalSize;

        /// <summary>
        /// Front of the buffer
        /// </summary>
        protected int Head;

        /// <summary>
        /// Back of the buffer
        /// </summary>
        protected int Tail;

        /// <summary>
        /// The buffer itself
        /// </summary>
        protected T[] Buffer;

        public int Capacity
        {
            get { return InternalCapacity; }
            set
            {
                if (value == InternalCapacity)
                    return;

                if (value < Size)
                    throw new ArgumentOutOfRangeException("value",
                        "Can't make maximum buffer capacity smaller than it's currently filled size!");
            }
        }

        public int Size
        {
            get { return InternalSize; }
        }

        public virtual T Peek()
        {
            return Buffer[Head];
        }

        public virtual void Enqueue(T obj)
        {
            Buffer[Tail] = obj;
            if (++Tail == Capacity)
                Tail = 0;
            InternalSize++;
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

            var item = Buffer[Head];
            if (++Head == Capacity)
                Head = 0;
            InternalSize--;
            return item;
        }

        public virtual IEnumerable<T> Dequeue(int count)
        {
            var availabileItems = Math.Min(count, Size);
            var returnItems = new List<T>(availabileItems);
            for (var i = 0; i < availabileItems; i++, Head++)
            {
                if (Head == Capacity)
                    Head = 0;
                returnItems.Add(Buffer[Head]);
            }
            InternalSize -= availabileItems;
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
            Head = 0;
            Tail = 0;
            InternalSize = 0;
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
            return ((IEnumerable<T>) ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        public int Count
        {
            get { return Size; }
        }

        public bool IsReadOnly { get; private set; }
        public virtual object SyncRoot { get; private set; }

        public virtual bool IsSynchronized
        {
            get { return false; }
        }
    }
}
