// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Helios.Util.Collections
{
    public class PriorityQueue<T> : IEnumerable<T>
    {
        private readonly IComparer<T> _comparer;
        private int _capacity;
        private T[] _items;

        public PriorityQueue(IComparer<T> comparer)
        {
            Contract.Requires(comparer != null);

            this._comparer = comparer;
            _capacity = 11;
            _items = new T[_capacity];
        }

        public PriorityQueue()
            : this(Comparer<T>.Default)
        {
        }

        public int Count { get; private set; }

        /// <summary>
        /// TODO: need to make this return the priority queue contents in the correct order
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Dequeue()
        {
            var result = Peek();
            if (result == null)
            {
                return default(T);
            }

            var newCount = --Count;
            var lastItem = _items[newCount];
            _items[newCount] = default(T);
            if (newCount > 0)
            {
                TrickleDown(0, lastItem);
            }

            return result;
        }

        public T Peek()
        {
            return Count == 0 ? default(T) : _items[0];
        }

        public void Enqueue(T item)
        {
            Contract.Requires(item != null);

            var oldCount = Count;
            if (oldCount == _capacity)
            {
                GrowHeap();
            }
            Count = oldCount + 1;
            BubbleUp(oldCount, item);
        }

        public void Remove(T item)
        {
            var index = Array.IndexOf(_items, item);
            if (index == -1)
            {
                return;
            }

            Count--;
            if (index == Count)
            {
                _items[index] = default(T);
            }
            else
            {
                var last = _items[Count];
                _items[Count] = default(T);
                TrickleDown(index, last);
                if (Equals(_items[index], last))
                {
                    BubbleUp(index, last);
                }
            }
        }

        private void BubbleUp(int index, T item)
        {
            // index > 0 means there is a parent
            while (index > 0)
            {
                var parentIndex = (index - 1) >> 1;
                var parentItem = _items[parentIndex];
                if (_comparer.Compare(item, parentItem) >= 0)
                {
                    break;
                }
                _items[index] = parentItem;
                index = parentIndex;
            }
            _items[index] = item;
        }

        private void GrowHeap()
        {
            var oldCapacity = _capacity;
            _capacity = oldCapacity + (oldCapacity <= 64 ? oldCapacity + 2 : oldCapacity >> 1);
            var newHeap = new T[_capacity];
            Array.Copy(_items, 0, newHeap, 0, Count);
            _items = newHeap;
        }

        private void TrickleDown(int index, T item)
        {
            var middleIndex = Count >> 1;
            while (index < middleIndex)
            {
                var childIndex = (index << 1) + 1;
                var childItem = _items[childIndex];
                var rightChildIndex = childIndex + 1;
                if (rightChildIndex < Count
                    && _comparer.Compare(childItem, _items[rightChildIndex]) > 0)
                {
                    childIndex = rightChildIndex;
                    childItem = _items[rightChildIndex];
                }
                if (_comparer.Compare(item, childItem) <= 0)
                {
                    break;
                }
                _items[index] = childItem;
                index = childIndex;
            }
            _items[index] = item;
        }

        public void Clear()
        {
            Count = 0;
            Array.Clear(_items, 0, 0);
        }
    }
}