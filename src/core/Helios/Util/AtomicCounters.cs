using System.Threading;

namespace Helios.Util
{
    /// <summary>
    /// Atomic counter that uses longs internally
    /// </summary>
    public class AtomicCounterLong
    {
         public AtomicCounterLong(long seed)
        {
            _seed = seed;
        }

         private long _seed;

        /// <summary>
        /// Retrieves the current value of the counter
        /// </summary>
         public long Current { get { return _seed; } }

        /// <summary>
        /// Increments the counter and returns the next value
        /// </summary>
         public long Next
        {
            get
            {
                return Interlocked.Increment(ref _seed);
            }
        }

        /// <summary>
        /// Returns the current value while simultaneously incrementing the counter
        /// </summary>
        public long GetAndIncrement()
        {
            var rValue = Current;
            var nextValue = Next;
            return rValue;
        }
    }

    /// <summary>
    /// Class used for atomic counters and increments.
    /// </summary>
    public class AtomicCounter
    {
        public AtomicCounter(int seed)
        {
            _seed = seed;
        }

        private int _seed;

        /// <summary>
        /// Retrieves the current value of the counter
        /// </summary>
        public int Current
        {
            get { return _seed; }
        }

        /// <summary>
        /// Increments the counter and returns the next value
        /// </summary>
        public int Next
        {
            get { return Interlocked.Increment(ref _seed); }
        }

        /// <summary>
        /// Returns the current value while simultaneously incrementing the counter
        /// </summary>
        public int GetAndIncrement()
        {
            var rValue = Current;
            var nextValue = Next;
            return rValue;
        }
    }
}
