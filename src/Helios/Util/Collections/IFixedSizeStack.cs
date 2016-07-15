// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections;
using System.Collections.Generic;

namespace Helios.Util.Collections
{
    public interface IFixedSizeStack<T> : IEnumerable<T>, ICollection
    {
        int Capacity { get; }
        T Peek();
        T Pop();
        void Push(T item);
        T[] ToArray();
        void Clear();

        /// <summary>
        ///     Copies the contents of the FixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        /// <param name="count">The number of items to copy from the current buffer (max value = current Size of buffer)</param>
        void CopyTo(T[] array, int index, int count);

        /// <summary>
        ///     Copies the contents of the FixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        void CopyTo(T[] array);

        /// <summary>
        ///     Copies the contents of the FixedSizeStack into a new array
        /// </summary>
        /// <param name="array">The destination array for the copy</param>
        /// <param name="index">The starting index for copying in the destination array</param>
        void CopyTo(T[] array, int index);
    }
}