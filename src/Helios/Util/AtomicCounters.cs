// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;

namespace Helios.Util
{
    /// <summary>
    ///     Atomic counter that uses longs internally
    /// </summary>
    public class AtomicCounterLong
    {
        private long _seed;

        public AtomicCounterLong(long seed)
        {
            _seed = seed;
        }

        /// <summary>
        ///     Retrieves the current value of the counter
        /// </summary>
        public long Current
        {
            get { return _seed; }
        }

        /// <summary>
        ///     Increments the counter and returns the next value
        /// </summary>
        public long Next
        {
            get { return Interlocked.Increment(ref _seed); }
        }

        /// <summary>
        ///     Returns the current value while simultaneously incrementing the counter
        /// </summary>
        public long GetAndIncrement()
        {
            var rValue = Current;
            var nextValue = Next;
            return rValue;
        }
    }

    /// <summary>
    ///     Class used for atomic counters and increments.
    ///     Used inside the <see cref="FSM{TS,TD}" /> and in parts of Akka.Remote.
    /// </summary>
    public class AtomicCounter
    {
        private int _seed;

        public AtomicCounter(int seed)
        {
            _seed = seed;
        }

        /// <summary>
        ///     Retrieves the current value of the counter
        /// </summary>
        public int Current
        {
            get { return _seed; }
        }

        /// <summary>
        ///     Increments the counter and returns the next value
        /// </summary>
        public int Next
        {
            get { return Interlocked.Increment(ref _seed); }
        }

        /// <summary>
        ///     Returns the current value while simultaneously incrementing the counter
        /// </summary>
        public int GetAndIncrement()
        {
            var rValue = Current;
            var nextValue = Next;
            return rValue;
        }
    }
}