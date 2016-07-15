// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;

namespace Helios.Util
{
    /// <summary>
    ///     Implementation of the java.concurrent.util AtomicReference type.
    ///     Uses <see cref="Interlocked.MemoryBarrier" /> internally to enforce ordering of writes
    ///     without any explicit locking. .NET's strong memory on write guarantees might already enforce
    ///     this ordering, but the addition of the MemoryBarrier guarantees it.
    /// </summary>
    public class AtomicReference<T> where T : class
    {
        // ReSharper disable once InconsistentNaming
        protected T atomicValue;

        /// <summary>
        ///     Sets the initial value of this <see cref="AtomicReference{T}" /> to <see cref="originalValue" />.
        /// </summary>
        public AtomicReference(T originalValue)
        {
            atomicValue = originalValue;
        }

        /// <summary>
        ///     Default constructor
        /// </summary>
        public AtomicReference()
        {
            atomicValue = default(T);
        }

        /// <summary>
        ///     The current value of this <see cref="AtomicReference{T}" />
        /// </summary>
        public T Value
        {
            get { return Volatile.Read(ref atomicValue); }
            set { Volatile.Write(ref atomicValue, value); }
        }

        #region Conversion operators

        /// <summary>
        ///     Implicit conversion operator = automatically casts the <see cref="AtomicReference{T}" /> to an instance of
        ///     <typeparam name="T"></typeparam>
        /// </summary>
        public static implicit operator T(AtomicReference<T> aRef)
        {
            return aRef.Value;
        }

        /// <summary>
        ///     Implicit conversion operator = allows us to cast any type directly into a <see cref="AtomicReference{T}" />
        ///     instance.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static implicit operator AtomicReference<T>(T newValue)
        {
            return new AtomicReference<T>(newValue);
        }

        #endregion
    }
}