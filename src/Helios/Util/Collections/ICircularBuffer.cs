// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Helios.Util.Collections
{
    public interface ICircularBuffer<T> : IProducerConsumerCollection<T>
    {
        /// <summary>
        ///     The gets the max capacity of the buffer
        /// </summary>
        int Capacity { get; }

        /// <summary>
        ///     The current size of the buffer.
        /// </summary>
        int Size { get; }

        /// <summary>
        ///     Peeks at the next message in the buffer
        /// </summary>
        /// <returns>The object at the start position of the buffer</returns>
        T Peek();

        /// <summary>
        ///     Adds an object to the end of the circular buffer
        /// </summary>
        /// <param name="obj">An object of type T</param>
        void Enqueue(T obj);

        /// <summary>
        ///     Adds an array of objects to the end of the circular buffer
        /// </summary>
        /// <param name="objs">An array of objects of type T</param>
        void Enqueue(T[] objs);

        /// <summary>
        ///     Dequeues an object from the start of the circular buffer
        /// </summary>
        /// <returns>An object of type T</returns>
        T Dequeue();

        /// <summary>
        ///     Dequeues multiple items at once, if available
        /// </summary>
        /// <param name="count">The maximum number of items to dequeue</param>
        /// <returns>An enumerable list of items</returns>
        IEnumerable<T> Dequeue(int count);

        /// <summary>
        ///     Dequeues the entire buffer in one dump
        /// </summary>
        /// <returns>All of the active contents of a circular buffer</returns>
        IEnumerable<T> DequeueAll();

        /// <summary>
        ///     Clears the contents from the buffer
        /// </summary>
        void Clear();

        /// <summary>
        ///     Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        void CopyTo(T[] array);

        /// <summary>
        ///     Copies the contents of the Circular Buffer into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        void CopyTo(T[] array, int index, int count);
    }
}