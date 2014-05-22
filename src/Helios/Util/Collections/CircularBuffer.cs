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
        /// <summary>
        /// Non-expanding circular buffer
        /// </summary>
        public CircularBuffer(int capacity) : this(capacity, capacity) { }

        public CircularBuffer(int capacity, int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            InternalCapacity = capacity;
            InternalSize = 0;
            Head = 0;
            Tail = 0;
            Buffer = new T[Capacity];
        }

        /// <summary>
        /// Thesize of the buffer
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

        public int MaxCapacity { get; private set; }

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
                if (value > InternalCapacity && InternalCapacity < MaxCapacity)
                {
                    var newCapacity = CalculateNewCapacity(value);
                    Expand(newCapacity);
                    InternalCapacity = newCapacity;
                }
            }
        }

        /// <summary>
        /// Grow the capacity by a power of two, so we aren't having to constantly expand it over and over again
        /// during periods of sustained writes.
        /// </summary>
        /// <param name="minNewCapacity">The minimum additional space we need to accomodiate</param>
        protected int CalculateNewCapacity(int minNewCapacity)
        {
            var maxCapacity = MaxCapacity;
            var threshold = 1048576; // 1 MiB page if sizeof(T) == 1 byte.
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

            // Not over threshold. Double up to 4 MiB, starting from 64.
            newCapacity = 64;
            while (newCapacity < minNewCapacity)
            {
                newCapacity <<= 1;
            }

            return Math.Min(newCapacity, maxCapacity);
        }

        /// <summary>
        /// Expands the circular buffer to accomodate additional space
        /// </summary>
        public virtual void Expand(int newSize)
        {
            var newBuffer = new T[newSize];
            CopyTo(newBuffer);
            Buffer = newBuffer;
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

            if (InternalSize + 1 >= Capacity)
            {
                Capacity += 1; //expand by 1 (or no-op if expansion isn't supported)
            }

            if (++Tail == Capacity)
                Tail = 0;
            InternalSize++;
        }

        public void Enqueue(T[] objs)
        {
            //Expand
            if (InternalSize + objs.Length >= Capacity)
                Capacity = InternalCapacity + objs.Length;

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
