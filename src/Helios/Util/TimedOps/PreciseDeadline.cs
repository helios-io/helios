// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Util.TimedOps
{
    /// <summary>
    ///     A <see cref="Deadline" /> alternative which relies on the <see cref="MonotonicClock" /> internally.
    /// </summary>
    public struct PreciseDeadline : IComparable<PreciseDeadline>
    {
        public PreciseDeadline(TimeSpan timespan) : this(timespan.Ticks + MonotonicClock.GetTicks())
        {
        }

        public PreciseDeadline(long tickCountDue)
        {
            When = tickCountDue;
        }

        public long When { get; }

        public bool IsOverdue => MonotonicClock.GetTicks() > When;

        #region Equality members

        public bool Equals(PreciseDeadline other)
        {
            return When == other.When;
        }

        public int CompareTo(PreciseDeadline other)
        {
            return When.CompareTo(other.When);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PreciseDeadline && Equals((PreciseDeadline) obj);
        }

        public override int GetHashCode()
        {
            return When.GetHashCode();
        }

        public static bool operator ==(PreciseDeadline left, PreciseDeadline right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PreciseDeadline left, PreciseDeadline right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Static members

        public static PreciseDeadline Now => new PreciseDeadline(MonotonicClock.GetTicks());
        public static PreciseDeadline MinusOne = new PreciseDeadline(-1);

        public static readonly PreciseDeadline Never = new PreciseDeadline(DateTime.MaxValue.Ticks);

        public static readonly PreciseDeadline Zero = new PreciseDeadline(0);

        /// <summary>
        ///     Adds a given <see cref="TimeSpan" /> to the due time of this <see cref="PreciseDeadline" />
        /// </summary>
        public static PreciseDeadline operator +(PreciseDeadline deadline, TimeSpan duration)
        {
            return deadline == Never ? deadline : new PreciseDeadline(deadline.When + duration.Ticks);
        }

        /// <summary>
        ///     Adds a given <see cref="Nullable{TimeSpan}" /> to the due time of this <see cref="PreciseDeadline" />
        /// </summary>
        public static PreciseDeadline operator +(PreciseDeadline deadline, TimeSpan? duration)
        {
            if (duration.HasValue)
                return deadline + duration.Value;
            return deadline;
        }

        /// <summary>
        ///     Adds a given <see cref="TimeSpan" /> to the due time of this <see cref="PreciseDeadline" />
        /// </summary>
        public static PreciseDeadline operator -(PreciseDeadline deadline, TimeSpan duration)
        {
            return deadline == Zero ? deadline : new PreciseDeadline(deadline.When - duration.Ticks);
        }

        /// <summary>
        ///     Adds a given <see cref="Nullable{TimeSpan}" /> to the due time of this <see cref="PreciseDeadline" />
        /// </summary>
        public static PreciseDeadline operator -(PreciseDeadline deadline, TimeSpan? duration)
        {
            if (duration.HasValue)
                return deadline - duration.Value;
            return deadline;
        }

        public static bool operator >(PreciseDeadline deadline1, PreciseDeadline deadline2)
        {
            return deadline1.When > deadline2.When;
        }

        public static bool operator >=(PreciseDeadline deadline1, PreciseDeadline deadline2)
        {
            return deadline1.When >= deadline2.When;
        }

        public static bool operator <(PreciseDeadline deadline1, PreciseDeadline deadline2)
        {
            return deadline1.When < deadline2.When;
        }

        public static bool operator <=(PreciseDeadline deadline1, PreciseDeadline deadline2)
        {
            return deadline1.When <= deadline2.When;
        }

        #endregion
    }
}