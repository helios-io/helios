// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

namespace Helios.Buffers
{
    /// <summary>
    ///     Reference counting interface for reusable objects
    /// </summary>
    public interface IReferenceCounted
    {
        /// <summary>
        ///     Returns the reference count of this object
        /// </summary>
        int ReferenceCount { get; }

        /// <summary>
        ///     Increases the reference count by 1
        /// </summary>
        IReferenceCounted Retain();

        /// <summary>
        ///     Increases the reference count by <see cref="increment" />.
        /// </summary>
        IReferenceCounted Retain(int increment);

        IReferenceCounted Touch();

        IReferenceCounted Touch(object hint);

        /// <summary>
        ///     Decreases the reference count by 1 and deallocates this object if the reference count reaches 0.
        /// </summary>
        /// <returns>true if and only if the reference count is 0 and this object has been deallocated</returns>
        bool Release();

        /// <summary>
        ///     Decreases the reference count by <see cref="decrement" /> and deallocates this object if the reference count
        ///     reaches 0.
        /// </summary>
        /// <returns>true if and only if the reference count is 0 and this object has been deallocated</returns>
        bool Release(int decrement);
    }

    /// <summary>
    ///     Exception thrown during instances where a reference count is used incorrectly
    /// </summary>
    public class IllegalReferenceCountException : HeliosException
    {
        public IllegalReferenceCountException(int count)
            : base(string.Format("Illegal reference count of {0} for this object", count))
        {
        }

        public IllegalReferenceCountException(int count, int increment)
            : base(
                string.Format("Illegal reference count of {0} for this object; was attempting to increment by {1}",
                    count, increment))
        {
        }
    }
}