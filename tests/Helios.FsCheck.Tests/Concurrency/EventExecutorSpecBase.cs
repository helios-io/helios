using System;
using System.Collections.Concurrent;
using FsCheck.Experimental;
using Helios.Concurrency;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FsCheck;

namespace Helios.FsCheck.Tests.Concurrency
{
    /// <summary>
    /// Non-thread-safe counter instance.
    /// Mutable
    /// </summary>
    public class SpecCounter
    {
        public SpecCounter(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }

        public SpecCounter IncrementBy(int value)
        {
            Value = Value + value;
            return this;
        }

        public SpecCounter Reset()
        {
            Value = 0;
            return this;
        }
    }

    public class CounterModel
    {
        public CounterModel(int currentValue, int nextValue, IEventExecutor executor)
        {
            CurrentValue = currentValue;
            NextValue = nextValue;
            Executor = executor;
        }

        /// <summary>
        /// Used for generating a counter instance
        /// </summary>
        public int CurrentValue { get; }

        /// <summary>
        /// Used for asserting post conditions
        /// </summary>
        public int NextValue { get; }

        public IEventExecutor Executor { get; }

        public CounterModel Next(int nextValue)
        {
            return new CounterModel(NextValue, nextValue, Executor);
        }

        public override string ToString()
        {
            return $"CurrentValue={CurrentValue}, NextValue={NextValue}";
        }
    }

    /// <summary>
    /// A note about this spec - it's designed to guarantee that the underlying <see cref="IEventExecutor"/> obeys FIFO order, at all times.
    /// 
    /// All of the operations we use to test, addition and subtraction, obey the commutative property. So all valid schedulings of those
    /// operations will produce the same result. We designed the test this way to avoid having to exhaustively calculate all possible orderings
    /// when interleaving multiple different types of operations.
    /// </summary>
    public abstract class EventExecutorSpecBase : Machine<SpecCounter, CounterModel>
    {
        protected EventExecutorSpecBase(IEventExecutor executor)
        {
            Executor = executor;
        }



        /// <summary>
        /// The <see cref="IEventExecutor"/> implementation that we will be testing. Created externally.
        /// </summary>
        public IEventExecutor Executor { get; }

        public override Gen<Operation<SpecCounter, CounterModel>> Next(CounterModel obj0)
        {
            return Gen.OneOf(Increment.IncrementGen(), Reset.ResetGen());
        }

        public override Arbitrary<Setup<SpecCounter, CounterModel>> Setup => Arb.From(Gen.Choose(0, int.MaxValue)
            .Select(i => (Setup<SpecCounter, CounterModel>)new ExecutorSetup(i, Executor)));

        #region Commands

        internal class ExecutorSetup : Setup<SpecCounter, CounterModel>
        {
            private int _seed;
            private readonly IEventExecutor _executor;

            public ExecutorSetup(int seed, IEventExecutor executor)
            {
                _seed = seed;
                _executor = executor;
            }

            public override SpecCounter Actual()
            {
                return new SpecCounter(0);
            }

            public override CounterModel Model()
            {
                return new CounterModel(0, 0, _executor);
            }

            public override string ToString()
            {
                return "Setup()";
            }
        }

        internal class Increment : Operation<SpecCounter, CounterModel>
        {
            public static Gen<Operation<SpecCounter, CounterModel>> IncrementGen()
            {
                return Gen.ArrayOf<int>(Arb.Default.Int32().Generator).Select(incs => (Operation<SpecCounter, CounterModel>)new Increment(incs));
            }

            public Increment(int[] increments)
            {
                Increments = increments;
            }

            public int[] Increments { get; set; }

            public int ExpectedDiff => Increments.Sum();

            public override Property Check(SpecCounter obj0, CounterModel obj1)
            {
                var tasks = new ConcurrentBag<Task>();
                Func<object, SpecCounter> incrementFunc = o => obj0.IncrementBy((int)o);
                var loopResult = Parallel.ForEach(Increments, inc =>
                {
                    tasks.Add(obj1.Executor.SubmitAsync(incrementFunc, inc));
                });

                SpinWait.SpinUntil(() => loopResult.IsCompleted, TimeSpan.FromMilliseconds(100));

                if (!Task.WhenAll(tasks).Wait(200))
                    return false.ToProperty().Label($"TIMEOUT: {obj1.Executor} failed to execute {Increments.Length} within 200ms");

                return (obj0.Value == obj1.NextValue).ToProperty().Label($"Actual counter value: [{obj0.Value}] should equal next predicted model value [{obj1.NextValue}]");
            }

            public override CounterModel Run(CounterModel obj0)
            {
                return obj0.Next(obj0.NextValue + ExpectedDiff);
            }

            public override string ToString()
            {
                return $"Increment(Incs = {string.Join(",", Increments)}, TotalDiff={ExpectedDiff})";
            }
        }

        internal class Reset : Operation<SpecCounter, CounterModel>
        {
            public static Gen<Operation<SpecCounter, CounterModel>> ResetGen()
            {
                return Gen.Constant((Operation<SpecCounter, CounterModel>)new Reset());
            }

            public override Property Check(SpecCounter obj0, CounterModel obj1)
            {
                Func<SpecCounter> resetFunc = obj0.Reset;
                if (!obj1.Executor.SubmitAsync(resetFunc).Wait(200))
                    return false.ToProperty().Label($"TIMEOUT: {obj1.Executor} failed to execute SpecCounter.Reset() within 200ms");
                return (obj0.Value == obj1.NextValue).ToProperty().Label($"Actual counter value: [{obj0.Value}] should equal next predicted model value [{obj1.CurrentValue}]"); ;
            }

            public override CounterModel Run(CounterModel obj0)
            {
                return obj0.Next(0);
            }
        }

        #endregion
    }
}
