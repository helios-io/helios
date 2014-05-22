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

        /// <summary>
        /// Expands the circular buffer to accomodate additional space
        /// </summary>
        public virtual void Expand(int newSize)
        {
            var previousSize = InternalSize; //number of elements in the array
            var newBuffer = new T[newSize];
            CopyTo(newBuffer);
            Buffer = newBuffer;
            Head = 0;
            Tail = previousSize;
        }

        public int Size
        {
            get { return InternalSize; }
        }

        public virtual T Peek()
        {
            if (Size == 0)
                return default(T);
            return Buffer[(Head % Capacity)];
        }

        /// <summary>
        /// Checks an index relative to the <see cref="Head"/> to see if there's a set element there
        /// </summary>
        public bool IsElementAt(int index)
        {
            return ((Head + index) % Capacity) <= Size;
        }

        public T ElementAt(int index)
        {
            return Buffer[(Head + index)%Capacity];
        }

        /// <summary>
        /// Sets an element at the specified position relative to <see cref="Head"/>
        /// WITHOUT MODIFYING <see cref="Tail"/>
        /// </summary>
        /// <param name="element">The element we want to add at the specified <see cref="index"/></param>
        /// <param name="index">The index relative to the front of the buffer where we want to add <see cref="element"/></param>
        public void SetElementAt(T element, int index)
        {
            Buffer[(Head + index) % Capacity] = element;
        }

        public T this[int index]
        {
            get { return ElementAt(index); }
            set { SetElementAt(value, index); }
        }

        public virtual void Enqueue(T obj)
        {
            if (InternalSize + 1 > Capacity)
            {
                Capacity += 1; //expand by 1 (or no-op if expansion isn't supported)
            }

            Buffer[Tail % Capacity] = obj;

            Tail++;

            if (InternalSize < Capacity) //overflowed, time to wrap around
                InternalSize++;
        }

        public void Enqueue(T[] objs)
        {
            //Expand
            if (InternalSize + objs.Length >= Capacity)
                Capacity += objs.Length;

            foreach (var item in objs)
            {
                Buffer[Tail % Capacity] = item;
                Tail++;
                if (InternalSize < Capacity) //overflowed, time to wrap around
                    InternalSize++;
            }
        }

        public virtual T Dequeue()
        {
            if (Size == 0)
                return default(T);

            var item = Buffer[Head % Capacity];
            Head++;
            InternalSize--;
            return item;
        }

        public virtual IEnumerable<T> Dequeue(int count)
        {
            var availabileItems = Math.Min(count, Size);
            var returnItems = new List<T>(availabileItems);
            for (var i = 0; i < availabileItems; i++, Head++, InternalSize--)
            {
                returnItems.Add(Buffer[Head % Capacity]);
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
            if(srcIndex + srcLength > src.Length) throw new ArgumentOutOfRangeException(string.Format("srcIndex: {0}, srcLength: {1} - expected srcIndex + srcLength to be less than src.Length ({2}", srcIndex, srcLength, src.Length));
            if (InternalSize + srcLength >= Capacity)
                Capacity += srcLength; //expand
           
            //check to see if we have enough contiguous space from the current Tail to the end of the buffer
            var physicalTailPos = (Tail%Capacity);
            if (physicalTailPos + srcLength < Capacity) //we can fit this inside one contiguous write
            {
                Array.Copy(src, srcIndex, Buffer, (Tail%Capacity), srcLength);
            }
            else //have to commit this write into two phases and wrap-around
            {
                var firstCommitLength = Capacity - physicalTailPos;
                var secondCommitLength = srcLength - firstCommitLength;
                Array.Copy(src, srcIndex, Buffer, physicalTailPos, firstCommitLength);
                Array.Copy(src, srcIndex + firstCommitLength, Buffer, 0, secondCommitLength);
            }

            Tail += srcLength;
            InternalSize += srcLength;
            if (InternalSize > Capacity) InternalSize = Capacity; //wrapped around
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
            if (destIndex + destLength > dest.Length) throw new ArgumentOutOfRangeException(string.Format("destIndex: {0}, destLength: {1} - expected destIndex + srcLength to be less than dest.Length ({2})", destIndex, destLength, dest.Length));
            if(InternalSize < destLength) throw new ArgumentOutOfRangeException(string.Format("destLength: {0}, expected destLength to be less than or equal to the size of the buffer ({1})", destLength, Size));

            //check to see if we have enough contiguous space from the current Head to the end of the buffer
            var physicalHeadPos = (Head % Capacity);
            if (physicalHeadPos + destLength < Capacity) //We have enough room to perform a contiguous read
            {
                Array.Copy(Buffer, physicalHeadPos, dest, destIndex, destLength);
            }
            else //we have to wrap around and perform the read in two steps
            {
                var firstCommitLength = Capacity - physicalHeadPos;
                var secondCommitLength = destLength - firstCommitLength;
                Array.Copy(Buffer, physicalHeadPos, dest, destIndex, firstCommitLength);
                Array.Copy(Buffer, 0, dest, destIndex + firstCommitLength, secondCommitLength);
            }

            Head += destLength;
            InternalSize -= destLength;
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
