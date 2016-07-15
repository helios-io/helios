// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Helios.Util.Collections
{
    /// <summary>
    ///     Stack with a fixed size number of members - old items get pushed
    ///     off the stack
    /// </summary>
    public class FixedSizeStack<T> : IFixedSizeStack<T>
    {
        /// <summary>
        ///     Default capacity for fixed size stacks
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public const int DEFAULT_CAPACITY = 10;

        public FixedSizeStack() : this(DEFAULT_CAPACITY)
        {
        }

        public FixedSizeStack(int capacity)
        {
            Capacity = capacity;
            _size = 0;
            _head = 0;
            _buffer = new T[capacity];
        }

        #region Properties

        public int Capacity { get; }

        #endregion

        #region Internal Fields

        protected int _size;
        protected int _head;

        /// <summary>
        ///     The buffer itself
        /// </summary>
        protected T[] _buffer;

        #endregion

        #region Stack Methods

        public virtual T Peek()
        {
            var item = default(T);

            if (Count > 0)
                return _buffer[_head%Capacity];

            return item;
        }

        public virtual T Pop()
        {
            var item = default(T);

            if (Count > 0)
            {
                item = _buffer[_head%Capacity];
                if (_head > 0) //Don't decrement the head if it's zero
                    _head--;
                _size--;
            }

            return item;
        }

        public virtual void Push(T item)
        {
            if (Count > 0)
                _head++;

            _buffer[_head%Capacity] = item;

            if (Count < Capacity)
                _size++;
        }

        public virtual T[] ToArray()
        {
            var resultArray = new T[_size];
            var availableItems = _size;
            var head = _head;

            for (var i = 0; i < availableItems; i++, head--)
            {
                resultArray[i] = _buffer[head%Capacity];
            }

            return resultArray;
        }

        public virtual void Clear()
        {
            _size = 0;
            _head = 0;
        }

        #endregion

        #region IEnumerable<T> members

        public virtual IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) ToArray()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ICollection members

        /// <summary>
        ///     Copies the contents of the FixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        public virtual void CopyTo(T[] array, int index, int count)
        {
            //Make a copy of the internal buffer and pointers
            var internalCopy = new T[Capacity];
            var internalHead = 0;
            var internalSize = 0;

            _buffer.CopyTo(internalCopy, 0);
            internalHead = _head;
            internalSize = _size;

            if (count > internalSize) //The maximum value of count is Size
                count = internalSize;

            for (var i = 0; i < count; i++, internalHead++, index++)
            {
                array[index] = internalCopy[internalHead];
            }
        }

        /// <summary>
        ///     Copies the contents of the FixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        public virtual void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        ///     Copies the contents of the FixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        public virtual void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, Count);
        }

        public virtual void CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        public virtual int Count
        {
            get { return _size; }
        }

        public virtual object SyncRoot { get; private set; }

        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        #endregion
    }
}