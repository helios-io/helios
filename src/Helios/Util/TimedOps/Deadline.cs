using System;

namespace Helios.Util.TimedOps
{
    /// <summary>
    /// Import of the scala.concurrent.duration.Deadline class
    /// </summary>
    public class Deadline
    {
        public Deadline(DateTime when)
        {
            When = when;
        }

        public bool IsOverdue
        {
            get { return DateTime.Now > When; }
        }

        public bool HasTimeLeft
        {
            get { return DateTime.Now < When; }
        }

        public DateTime When { get; private set; }

        #region Overrides

        public override bool Equals(object obj)
        {
            var deadlineObj = obj as Deadline;
            if (deadlineObj == null)
            {
                return false;
            }

            return When.Equals(deadlineObj.When);
        }

        public override int GetHashCode()
        {
            return When.GetHashCode();
        }

        #endregion


        #region Static members

        /// <summary>
        /// Returns a deadline that is due <see cref="DateTime.Now"/>
        /// </summary>
        public static Deadline Now
        {
            get
            {
                return new Deadline(DateTime.Now);
            }
        }

        public static Deadline Never
        {
            get
            {
                return new Deadline(DateTime.MaxValue);
            }
        }

        /// <summary>
        /// Adds a given <see cref="TimeSpan"/> to the due time of this <see cref="Deadline"/>
        /// </summary>
        public static Deadline operator +(Deadline deadline, TimeSpan duration)
        {
            return deadline.When == DateTime.MaxValue ? deadline : new Deadline(deadline.When.Add(duration));
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
