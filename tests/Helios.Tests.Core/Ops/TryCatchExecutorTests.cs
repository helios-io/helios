using System;
using System.Linq;
using Helios.Core.Ops.Executors;
using Helios.Core.Util;
using NUnit.Framework;

namespace Helios.Tests.Core.Ops
{
    public class TryCatchExecutorTests : BasicExecutorTests
    {
        #region Setup / Teardown

        public override void SetUp()
        {
            Executor = new TryCatchExecutor();
        }

        #endregion

        #region Tests

        [Test]
        public void Should_report_exception_when_thrown()
        {
            //arrange
            var handledException = false;
            Action<Exception> exCallback = exception => { handledException = true; };
            Action exOperation = () => { throw new Exception("Plain old exception"); };
            Executor = new TryCatchExecutor(exCallback);

            //act
            Executor.Execute(exOperation);

            //assert
            Assert.IsTrue(handledException);
        }

        [Test]
        public void Should_not_report_exception_when_not_thrown()
        {
            //arrange
            var handledException = false;
            var called = false;
            Action<Exception> exCallback = exception => { handledException = true; };
            Action exOperation = () => { called = true; };
            Executor = new TryCatchExecutor(exCallback);

            //act
            Executor.Execute(exOperation);

            //assert
            Assert.IsFalse(handledException);
            Assert.IsTrue(called);
        }

        [Test]
        public void Should_pipe_remaining_operations_when_exception_thrown()
        {
            //arrange
            var handledException = false;
            var remainingJobsCalled = true;
            var remainingJobsCount = 0;
            Action<Exception> exCallback = exception => { handledException = true; };
            Action exOperation = () => { throw new Exception("Plain old exception"); };
            Executor = new TryCatchExecutor(exCallback);

            var wasCalled = 3.Of(false).ToList();
            var callbacks = new Action[wasCalled.Count];

            for (var i = 0; i < wasCalled.Count; i++)
            {
                var i1 = i;
                callbacks[i] = new Action(() => { wasCalled[i1] = true; });
            }
            callbacks[1] = exOperation;

            //act
            Executor.Execute(callbacks, actions =>
            {
                remainingJobsCalled = true;
                remainingJobsCount = actions.Count();
            });

            //assert
            Assert.IsFalse(wasCalled.All(x => x));
            Assert.IsTrue(handledException);
            Assert.IsTrue(remainingJobsCalled);
            Assert.AreEqual(1, remainingJobsCount);
        }

        #endregion
    }
}