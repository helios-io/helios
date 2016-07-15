// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Helios.Util.Collections
{
    /// <summary>
    ///     Base class for working with circular buffers
    /// </summary>
    /// <typeparam name="T">The type being stored in the circular buffer</typeparam>
    public class CircularBuffer<T> : ICircularBuffer<T>
    {
        private bool _full;

        /// <summary>
        ///     Front of the buffer
        /// </summary>
        private int _head;

        /// <summary>
        ///     Back of the buffer
        /// </summary>
        private int _tail;

        /// <summary>
        ///     The buffer itself
        /// </summary>
        protected T[] Buffer;

        public CircularBuffer(int capacity)
        {
            _head = 0;
            _tail = 0;
            Buffer = new T[capacity];
        }

        /// <summary>
        ///     FOR TESTING PURPOSES ONLY
        /// </summary>
        internal int Head => _head;

        /// <summary>
        ///     FOR TESTING PURPOSES ONLY
        /// </summary>
        internal int Tail => _tail;

        public bool IsReadOnly { get; private set; }

        public int Capacity => Buffer.Length;

        // We use an N+1 trick here to make sure we can distinguish full queues from empty ones
        public int Size => _full ? Capacity : (_tail - _head + Capacity)%Capacity;

        public virtual T Peek()
        {
            return Buffer[_head];
        }

        public virtual void Enqueue(T obj)
        {
            _full = _full || _tail + 1 == Capacity; // leave FULL flag on
            Buffer[_tail] = obj;
            _tail = (_tail + 1)%Capacity;
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
            _head = (_head + 1)%Capacity;
            return item;
        }

        public virtual IEnumerable<T> Dequeue(int count)
        {
            var availabileItems = Math.Min(count, Size);
            var returnItems = new List<T>(availabileItems);
            for (var i = 0; i < availabileItems; i++)
            {
                returnItems.Add(Dequeue());
            }

            return returnItems;
        }

        public IEnumerable<T> DequeueAll()
        {
            return Dequeue(Size);
        }

        public virtual void Clear()
        {
            _head = 0;
            _tail = 0;
            _full = false;
        }

        /// <summary>
        ///     Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        ///     Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, Size);
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
        ///     Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        public void CopyTo(T[] array, int index, int count)
        {
            if (count > Size) //The maximum value of count is Size
                count = Size;

            var bufferBegin = _head;
            for (var i = 0; i < count; i++, bufferBegin = (bufferBegin + 1)%Capacity, index++)
            {
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

        public int Count => Size;
        public virtual object SyncRoot { get; private set; }

        public virtual bool IsSynchronized => false;

        public void Add(T item)
        {
            Enqueue(item);
        }

        public bool Contains(T item)
        {
            return Buffer.Any(x => x.GetHashCode() == item.GetHashCode());
        }

        public bool Remove(T item)
        {
            return false;
        }

        public override string ToString()
        {
            return $"CircularBuffer<{GetType()}>(Capacity = {Capacity})";
        }
    }
}