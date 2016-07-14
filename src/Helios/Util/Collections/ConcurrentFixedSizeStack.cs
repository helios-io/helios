// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Util.Collections
{
    /// <summary>
    ///     Stack with a fixed size number of members - old items get pushed
    ///     off the stack. Concurrent.
    /// </summary>
    public class ConcurrentFixedSizeStack<T> : FixedSizeStack<T>
    {
        #region Internal members

        private readonly object m_lockObject = new object();

        #endregion

        public ConcurrentFixedSizeStack()
        {
        }

        public ConcurrentFixedSizeStack(int capacity) : base(capacity)
        {
        }

        #region Stack Methods

        public override T Peek()
        {
            var item = default(T);

            lock (m_lockObject)
            {
                item = base.Peek();
            }

            return item;
        }

        public override T Pop()
        {
            T item;

            lock (m_lockObject)
            {
                item = base.Pop();
            }

            return item;
        }

        public override void Push(T item)
        {
            lock (m_lockObject)
            {
                base.Push(item);
            }
        }

        public override T[] ToArray()
        {
            lock (m_lockObject)
            {
                return base.ToArray();
            }
        }

        public override void Clear()
        {
            lock (m_lockObject)
            {
                base.Clear();
            }
        }

        #endregion

        #region ICollection methods

        /// <summary>
        ///     Copies the contents of the ConcurrentFixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        public override void CopyTo(T[] array, int index, int count)
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

            //Now we're thread-safe - have an internal copy to work with

            if (count > internalSize) //The maximum value of count is Size
                count = internalSize;

            for (var i = 0; i < count; i++, internalHead++, index++)
            {
                array[index] = internalCopy[internalHead];
            }
        }

        public override object SyncRoot
        {
            get { return m_lockObject; }
        }

        public override bool IsSynchronized
        {
            get { return true; }
        }

        #endregion
    }
}