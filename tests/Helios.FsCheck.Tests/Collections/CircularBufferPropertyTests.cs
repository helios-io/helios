using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using Helios.Util;
using Helios.Util.Collections;
using NUnit.Framework;

namespace Helios.FsCheck.Tests.Collections
{

    [TestFixture]
    public class CircularBufferTests
    {
        [Test]
        public void CircularBuffer_Should_Pass()
        {
            Func<int> generator = () => ThreadLocalRandom.Current.Next();
            var tests = new CircularBufferPropertyTests<int>(generator);
            tests.ToProperty().QuickCheckThrowOnFailure();
        }
    }

    public class CircularBufferPropertyTests<T> : ICommandGenerator<CircularBuffer<T>, IEnumerable<T>>
        
    {
        public Func<T> Generator { get; }

        public CircularBufferPropertyTests(Func<T> generator)
        {
            Generator = generator;
        }

        public Gen<Command<CircularBuffer<T>, IEnumerable<T>>> Next(IEnumerable<T> obj0)
        {
            return Gen.Elements(new Command<CircularBuffer<T>, IEnumerable<T>>[] {new Allocate(), new Enqueue(Generator), new Dequeue(), new Size(), });
        }

        public CircularBuffer<T> InitialActual => new CircularBuffer<T>(3); // no model yet - must be allocated as part of spec
        public IEnumerable<T> InitialModel => new List<T>(3); // no actual yet - must be allocated as part of spec

        private class Allocate : Command<CircularBuffer<T>, IEnumerable<T>>
        {
            private readonly Lazy<int> ListSize = new Lazy<int>(() => ThreadLocalRandom.Current.Next(1,10));

            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {
                return new CircularBuffer<T>(ListSize.Value);
            }

            public override IEnumerable<T> RunModel(IEnumerable<T> obj0)
            {
                return new List<T>(ListSize.Value);
            }

            public override string ToString()
            {
                return $"new CircularBuffer{typeof(T)}({ListSize.Value})";
            }

            public override bool Pre(IEnumerable<T> _arg1)
            {
                return _arg1 == null;
            }
        }

        private class Enqueue : Command<CircularBuffer<T>, IEnumerable<T>>
        {
            public Enqueue(Func<T> generator)
            {
                Generator = generator;
                _data = new Lazy<T>(Generator);
            }

            private Func<T> Generator { get; }

            private readonly Lazy<T> _data;

            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {
               obj0.Add(_data.Value);
                return obj0;
            }

            public override IEnumerable<T> RunModel(IEnumerable<T> obj0)
            {
                return obj0.Concat(new[] {_data.Value}).ToList();
            }

            public override bool Pre(IEnumerable<T> _arg1)
            {
                return _arg1 != null;
            }

            public override Property Post(CircularBuffer<T> _arg2, IEnumerable<T> _arg3)
            {
                return _arg2.Tail.Equals(_arg3.Reverse().First()).ToProperty();
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Enqueue({_data.Value})";
            }
        }

        private class Dequeue : Command<CircularBuffer<T>, IEnumerable<T>>
        {
            private T dequeValue;

            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {
                dequeValue = obj0.Dequeue();
                return obj0;
            }

            public override IEnumerable<T> RunModel(IEnumerable<T> obj0)
            {
                return obj0.Skip(1).ToList();
            }

            public override bool Pre(IEnumerable<T> _arg1)
            {
                return _arg1 != null && _arg1.Any();
            }

            public override Property Post(CircularBuffer<T> _arg2, IEnumerable<T> _arg3)
            {
                return (_arg2.Skip(1).SequenceEqual(_arg3.Skip(1))).ToProperty();
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Dequeue() => {dequeValue}";
            }
        }

        private class Size : Command<CircularBuffer<T>, IEnumerable<T>>
        {
            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {
                return obj0;
            }

            public override IEnumerable<T> RunModel(IEnumerable<T> obj0)
            {
                return obj0;
            }

            public override bool Pre(IEnumerable<T> _arg1)
            {
                return _arg1 != null;
            }

            public override Property Post(CircularBuffer<T> _arg2, IEnumerable<T> _arg3)
            {
                return (_arg2.Count == _arg3.Count()).ToProperty().Label($"Expected {_arg3.Count()}, got {_arg2.Count}");
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Size()";
            }
        }
    }
}
