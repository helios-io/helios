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
        where T : class
    {
        private readonly IComparer<T> comparer;
        private int capacity;
        private T[] items;

        public PriorityQueue(IComparer<T> comparer)
        {
            Contract.Requires(comparer != null);

            this.comparer = comparer;
            capacity = 11;
            items = new T[capacity];
        }

        public PriorityQueue()
            : this(Comparer<T>.Default)
        {
        }

        public int Count { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return items[i];
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
                return null;
            }

            var newCount = --Count;
            var lastItem = items[newCount];
            items[newCount] = null;
            if (newCount > 0)
            {
                TrickleDown(0, lastItem);
            }

            return result;
        }

        public T Peek()
        {
            return Count == 0 ? null : items[0];
        }

        public void Enqueue(T item)
        {
            Contract.Requires(item != null);

            var oldCount = Count;
            if (oldCount == capacity)
            {
                GrowHeap();
            }
            Count = oldCount + 1;
            BubbleUp(oldCount, item);
        }

        public void Remove(T item)
        {
            var index = Array.IndexOf(items, item);
            if (index == -1)
            {
                return;
            }

            Count--;
            if (index == Count)
            {
                items[index] = default(T);
            }
            else
            {
                var last = items[Count];
                items[Count] = default(T);
                TrickleDown(index, last);
                if (items[index] == last)
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
                var parentItem = items[parentIndex];
                if (comparer.Compare(item, parentItem) >= 0)
                {
                    break;
                }
                items[index] = parentItem;
                index = parentIndex;
            }
            items[index] = item;
        }

        private void GrowHeap()
        {
            var oldCapacity = capacity;
            capacity = oldCapacity + (oldCapacity <= 64 ? oldCapacity + 2 : oldCapacity >> 1);
            var newHeap = new T[capacity];
            Array.Copy(items, 0, newHeap, 0, Count);
            items = newHeap;
        }

        private void TrickleDown(int index, T item)
        {
            var middleIndex = Count >> 1;
            while (index < middleIndex)
            {
                var childIndex = (index << 1) + 1;
                var childItem = items[childIndex];
                var rightChildIndex = childIndex + 1;
                if (rightChildIndex < Count
                    && comparer.Compare(childItem, items[rightChildIndex]) > 0)
                {
                    childIndex = rightChildIndex;
                    childItem = items[rightChildIndex];
                }
                if (comparer.Compare(item, childItem) <= 0)
                {
                    break;
                }
                items[index] = childItem;
                index = childIndex;
            }
            items[index] = item;
        }

        public void Clear()
        {
            Count = 0;
            Array.Clear(items, 0, 0);
        }
    }
}

