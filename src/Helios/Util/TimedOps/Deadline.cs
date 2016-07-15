// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Util.TimedOps
{
    /// <summary>
    ///     Import of the scala.concurrent.duration.Deadline class
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

        public DateTime When { get; }

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
        ///     Returns a deadline that is due <see cref="DateTime.Now" />
        /// </summary>
        public static Deadline Now
        {
            get { return new Deadline(DateTime.Now); }
        }

        public static Deadline Never
        {
            get { return new Deadline(DateTime.MaxValue); }
        }

        /// <summary>
        ///     Adds a given <see cref="TimeSpan" /> to the due time of this <see cref="Deadline" />
        /// </summary>
        public static Deadline operator +(Deadline deadline, TimeSpan duration)
        {
            return deadline.When == DateTime.MaxValue ? deadline : new Deadline(deadline.When.Add(duration));
        }

        /// <summary>
        ///     Adds a given <see cref="Nullable{TimeSpan}" /> to the due time of this <see cref="Deadline" />
        /// </summary>
        public static Deadline operator +(Deadline deadline, TimeSpan? duration)
        {
            if (duration.HasValue)
                return deadline + duration.Value;
            return deadline;
        }

        #endregion
    }
}