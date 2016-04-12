using System;
using System.Linq;
using Helios.Ops.Executors;
using Helios.Util;
using Xunit;

namespace Helios.Tests.Ops
{
    public class TryCatchExecutorTests : BasicExecutorTests
    {
        #region Setup / Teardown

        public TryCatchExecutorTests()
        {
            Executor = new TryCatchExecutor();
        }

        #endregion

        #region Tests

        [Fact]
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
            Assert.True(handledException);
        }

        [Fact]
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
            Assert.False(handledException);
            Assert.True(called);
        }

        [Fact]
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
            Assert.False(wasCalled.All(x => x));
            Assert.True(handledException);
            Assert.True(remainingJobsCalled);
            Assert.Equal(1, remainingJobsCount);
        }

        #endregion
    }
}