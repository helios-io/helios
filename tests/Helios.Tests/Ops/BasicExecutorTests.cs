using System;
using System.Linq;
using System.Threading;
using Helios.Ops;
using Helios.Ops.Executors;
using Helios.Util;
using NUnit.Framework;

namespace Helios.Tests.Ops
{
    [TestFixture]
    public class BasicExecutorTests
    {
        #region Setup / Teardown

        protected IExecutor Executor;

        [SetUp]
        public virtual void SetUp()
        {
            Executor = new BasicExecutor();
        }

        #endregion

        #region Tests

        [Test]
        public void Should_execute_operation_when_AcceptingJobs()
        {
            //arrange
            bool wasCalled = false;
            var callback = new Action(() => { wasCalled = true; });

            //act
            Executor.Execute(callback);

            //assert
            Assert.IsTrue(Executor.AcceptingJobs);
            Assert.IsTrue(wasCalled);
        }

        [Test]
        public void Should_not_execute_operation_when_not_AcceptingJobs()
        {
            //arrange
            bool wasCalled = false;
            var callback = new Action(() => { wasCalled = true; });

            //act
            Executor.Shutdown(); //immediate shutdown
            Executor.Execute(callback);

            //assert
            Assert.IsFalse(Executor.AcceptingJobs);
            Assert.IsFalse(wasCalled);
        }

        [Test]
        public void Should_execute_queue_of_jobs_when_AcceptingJobs()
        {
            //arrange
            var wasCalled = 3.Of(false).ToList();
            var callbacks = new Action[wasCalled.Count];

            for (var i = 0; i < wasCalled.Count; i++)
            {
                var i1 = i;
                callbacks[i] = new Action(() => { wasCalled[i1] = true; });
            }

            //act
            Executor.Execute(callbacks, null);

            //assert
            Assert.IsTrue(Executor.AcceptingJobs);
            Assert.IsTrue(wasCalled.All(x => x));
        }

        [Test]
        public void Should_output_unfinished_jobs_when_AcceptJobs_disabled_during_execution()
        {
            //arrange
            var wasCalled = 3.Of(false).ToList();
            var remainingJobsCalled = false;
            var callbacks = new Action[wasCalled.Count];

            for (var i = 0; i < wasCalled.Count; i++)
            {
                var i1 = i;
                callbacks[i] = new Action(() => { wasCalled[i1] = true; });
            }

            var slowCallback = new Action(() => Thread.Sleep(TimeSpan.FromSeconds(2)));
            callbacks[1] = slowCallback; //stick the slow callback in the middle

            //act
            Executor.Shutdown(TimeSpan.FromSeconds(1));
            Executor.Execute(callbacks, actions => { remainingJobsCalled = true; });

            //assert
            Assert.IsFalse(Executor.AcceptingJobs);
            Assert.IsFalse(wasCalled.All(x => x));
            Assert.IsTrue(remainingJobsCalled);
        }

        #endregion
    }
}
