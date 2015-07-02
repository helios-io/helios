using System;

namespace Helios.Util.TimedOps
{
    /// <summary>
    /// Import of the scala.concurrent.duration.Deadline class
    /// </summary>
    public struct Deadline
    {
        /// <summary>
        /// Deadline that takes a tick count from the <see cref="MonotonicClock"/> as an argument.
        /// 
        /// <remarks>
        /// Probably best not to use this constructor directly. Instead, use <see cref="Deadline"/>'s addition operator.
        /// <code>
        ///     var deadline = Deadline.Now + TimeSpan.FromSeconds(0.5); // current time plus 500 milliseconds
        ///     var deadline2 = deadline + TimeSpan.FromMinutes(1); // previous deadline, plus 1 minute
        /// </code>
        /// </remarks>
        /// </summary>
        /// <param name="tickCount"></param>
        public Deadline(long tickCount)
        {
            When = tickCount;
        }

        /// <summary>
        /// Returns <c>true</c> when the <see cref="MonotonicClock.ElapsedTicks"/> is greater than <see cref="When"/>.
        /// 
        /// Indicates that the <see cref="Deadline"/> has now passed and is overdue.
        /// </summary>
        public bool IsOverdue
        {
            get { return MonotonicClock.ElapsedTicks > When; }
        }

        /// <summary>
        /// Opposite of <see cref="IsOverdue"/>. Equivalent of 
        /// <code>
        ///     var hasTimeLeft = !deadline.IsOverdue;
        /// </code>
        /// </summary>
        public bool HasTimeLeft
        {
            get { return MonotonicClock.ElapsedTicks < When; }
        }

        /// <summary>
        /// read-only representation of the elapsed time this <see cref="Deadline"/> will use before it's considered to be overdue.
        /// </summary>
        public readonly long When;

        #region Overrides

        public bool Equals(Deadline other)
        {
            return When == other.When;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Deadline && Equals((Deadline) obj);
        }

        public override int GetHashCode()
        {
            return When.GetHashCode();
        }

        public static bool operator ==(Deadline left, Deadline right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Deadline left, Deadline right)
        {
            return !left.Equals(right);
        }

        #endregion


        #region Static members

        /// <summary>
        /// Returns a <see cref="Deadline"/> that is due according to the current time from the <see cref="MonotonicClock"/>.
        /// </summary>
        /// <returns>
        /// a <see cref="Deadline"/> that is already due.
        /// </returns>
        public static Deadline Now
        {
            get
            {
                return new Deadline(MonotonicClock.ElapsedTicks);
            }
        }

        /// <summary>
        /// Returns a <see cref="Deadline"/> that is never due (uses <see cref="DateTime.MaxValue"/> under the hood.)
        /// </summary>
        /// <returns>a <see cref="Deadline"/> that is never due.</returns>
        public static Deadline Never
        {
            get
            {
                return new Deadline(DateTime.MaxValue.Ticks);
            }
        }

        /// <summary>
        /// Adds a given <see cref="TimeSpan"/> to the due time of this <see cref="Deadline"/>
        /// </summary>
        public static Deadline operator +(Deadline deadline, TimeSpan duration)
        {
            return deadline.When == DateTime.MaxValue.Ticks ? deadline : new Deadline(new TimeSpan(deadline.When).Add(duration).Ticks);
        }

        /// <summary>
        /// Adds a given <see cref="Nullable{TimeSpan}"/> to the due time of this <see cref="Deadline"/>
        /// </summary>
        public static Deadline operator +(Deadline deadline, TimeSpan? duration)
        {
            if (duration.HasValue)
                return deadline + duration.Value;
            else
                return deadline;
        }

        #endregion

    }
}
