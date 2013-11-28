using System;
using System.Timers;

namespace Helios.Core.Util.TimedOps
{
    /// <summary>
    /// When triggered, ScheduledValue will update an underlying
    /// value to a new one after a period of time has elapsed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScheduledValue<T> : IDisposable
    {
        protected Timer SetTimer;

        public ScheduledValue(T initialValue)
        {
            Value = initialValue;
        }

        public ScheduledValue(T initialValue, T futureValue, TimeSpan timeToSet) : this(initialValue)
        {
            Schedule(futureValue, timeToSet);
        }

        protected T FutureValue;

        public T Value { get; protected set; }

        public bool IsScheduled { get; protected set; }

        /// <summary>
        /// Indicates that the future value was successfully set
        /// </summary>
        public bool WasSet { get; set; }

        public bool WasDisposed { get; set; }

        public void Cancel()
        {
            if (SetTimer == null || !IsScheduled) return;
            SetTimer.Stop();
            SetTimer.Elapsed -= SetTimerOnElapsed;
            IsScheduled = false;
        }

        public void Schedule(T futureValue, TimeSpan timeToSet)
        {
            FutureValue = futureValue;

            if (SetTimer == null)
            {
                SetTimer = new Timer();
            }
            IsScheduled = true;
            SetTimer.Interval = timeToSet.TotalMilliseconds;
            SetTimer.Enabled = true;
            SetTimer.Elapsed += SetTimerOnElapsed;
        }

        private void SetTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Value = FutureValue;
            WasSet = true;
            IsScheduled = false;
            Cancel();
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        #region IDisposable Members

        public void Dispose(bool isDisposing)
        {
            if (SetTimer != null)
            {
                Cancel();
                SetTimer.Dispose();
            }

            WasDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
