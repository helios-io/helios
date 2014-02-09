using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Util.Concurrency
{
    /// <summary>
    /// Task scheduler that allows callers to use a limited-size thread pool
    /// </summary>
    public class FixedSizeThreadPoolTaskScheduler : TaskScheduler, IDisposable
    {
        private BlockingCollection<Task> _tasks = new BlockingCollection<Task>();
        private Thread[] _threads;

        public FixedSizeThreadPoolTaskScheduler(int maximumThreadCount)
        {
            maximumThreadCount.NotLessThan(0);
            _maximumConcurrencyLevel = maximumThreadCount;
            _threads = SpawnThreads(_maximumConcurrencyLevel);
        }

        private readonly int _maximumConcurrencyLevel;
        public override sealed int MaximumConcurrencyLevel
        {
            get { return _maximumConcurrencyLevel; }
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (_threads.Any(x => x == Thread.CurrentThread))
                return TryExecuteTask(task);
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }

        #region Thread management members

        public Thread[] SpawnThreads(int maxConcurrency)
        {
           return maxConcurrency.Times(SpawnThread).ToArray();
        }

        public Thread SpawnThread()
        {
            var t = new Thread(() =>
            {
                foreach (var task in _tasks.GetConsumingEnumerable())
                {
                    TryExecuteTask(task);
                }
            }) {IsBackground = true};
            t.Start();
            return t;
        }

        #endregion

        #region IDisposable members

        public void Dispose()
        {
            if (_threads != null)
            {
                _tasks.CompleteAdding(); //block until all operatings have finished
                foreach (var thread in _threads)
                {
                    thread.Join();
                }
                _tasks.Dispose();
                _tasks = null;
                _threads = null;
            }
        }

        ~FixedSizeThreadPoolTaskScheduler()
        {
            Dispose();
        }

        #endregion
    }
}
