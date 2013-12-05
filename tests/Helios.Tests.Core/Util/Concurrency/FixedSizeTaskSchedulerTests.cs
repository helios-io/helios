using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helios.Core.Util;
using Helios.Core.Util.Concurrency;
using NUnit.Framework;

namespace Helios.Tests.Core.Util.Concurrency
{
    [TestFixture]
    public class FixedSizeTaskSchedulerTests
    {
        #region Setup / Teardown

        private TaskFactory tf;
        private TaskScheduler ts;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            ts = new FixedSizeThreadPoolTaskScheduler(2);
            tf = new TaskFactory(ts);
        }

        #endregion

        #region Tests

        [Test]
        public void Should_execute_multiple_tasks_on_different_threads()
        {
            //arrange
            var taskThreadIds = 10.Of(0).ToList();
            var ops = new List<Action>();
            for (var i = 0; i < taskThreadIds.Count; i++)
            {
                var i1 = i;
                ops.Add(() => { taskThreadIds[i1] = Thread.CurrentThread.ManagedThreadId; });
               
            }

            //act
            var tasks = ops.Select(task => tf.StartNew(task)).ToList();
            Task.WaitAll(tasks.ToArray());

            //assert
            Assert.IsTrue(tasks.TrueForAll(x => x.IsCompleted));
            Assert.IsTrue(taskThreadIds.Distinct().Count() > 1);
        }

        #endregion
    }
}
