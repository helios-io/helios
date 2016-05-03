using System;

namespace Helios.Concurrency
{
    public interface IScheduledRunnable : IRunnable, IScheduledTask, IComparable<IScheduledRunnable> { }
}