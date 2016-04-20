using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Xunit;
using Helios.Util;

namespace Helios.FsCheck.Tests.Concurrency
{
    /// <summary>
    /// Property-based tests for <see cref="ObjectPool{T}"/>
    /// </summary>
    public class ObjectPoolSpec
    {
        class MyPooledObject
        {
            public int Num { get; set; }
            public int Num2 { get; set; }

            public void Recycle()
            {
                Num = 0;
                Num2 = 0;
            }
        }

        Func<MyPooledObject> _producer = () => new MyPooledObject();
        public static readonly int ObjectCount = Environment.ProcessorCount*2;
        private ObjectPool<MyPooledObject> _pool;

        public static Arbitrary<Tuple<int, int>> TestData()
        {
            var gen = (Gen.Two(Arb.Default.Int32().Generator));
            return Arb.From(gen);
        }

        public ObjectPoolSpec()
        {
            _pool = new ObjectPool<MyPooledObject>(_producer, ObjectCount);
            Arb.Register<ObjectPoolSpec>();
        }

        [Property(QuietOnSuccess = true, StartSize = 10)]
        public Property ObjectPool_should_not_leak_when_used_properly(Tuple<int,int>[] values)
        {
            var tasks = new List<Task<bool>>();
            var pooledObjects = new ConcurrentBag<MyPooledObject>();
            Func<Tuple<int,int>,MyPooledObject> setPool = tupe =>
            {
                MyPooledObject obj = _pool.Take();
                obj.Num = tupe.Item1;
                obj.Num2 = tupe.Item2;
                return obj;
            };

            Func<MyPooledObject, Tuple<int, int>, bool> freePoolAndAssertReferentialIntegrity = (o, tuple) =>
            {
                var propsEqual = o.Num == tuple.Item1 && o.Num2 == tuple.Item2;
                pooledObjects.Add(o); //add a reference to O
                _pool.Free(o);
                return propsEqual;
            };

            foreach (var value in values)
            {
                var v = value;
                var task = Task.Run(() => setPool(v)).ContinueWith(t => freePoolAndAssertReferentialIntegrity(t.Result, v));
                tasks.Add(task);
            }
            var results = Task.WhenAll(tasks);
            if (!results.Wait(200))
                return false.Label($"Should not have taken 200ms to process {values.Length} items");

            if (!results.Result.All(x => x))
                return false.Label("None of the objects in the pool should ever be concurrently modified while in use");

            var count = pooledObjects.Distinct().Count();
            return (count <= ObjectCount).Label($"Should not have produced more than {ObjectCount}, but was instead {count}");
        }
    }
}
