using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.Xunit;
using Helios.Util;
using Helios.Util.Collections;
using Xunit;

namespace Helios.FsCheck.Tests.Collections
{

    public class CircularBufferTests
    {
        public CircularBufferTests()
        {
            Arb.Register<HeliosGenerators>();
        }

        [Property(QuietOnSuccess = true)]
        public Property CircularBuffer_dequeue_after_enqueue_should_return_original_item(CircularBuffer<int> buffer, int[] itemsToAdd)
        {
            for (var i = 0; i < itemsToAdd.Length; i++)
            {
                buffer.Enqueue(itemsToAdd[i]);
                var dequeue = buffer.Dequeue();
                if (dequeue != itemsToAdd[i])
                {
                    return false.When(buffer.Capacity > 0).Label($"Failed with {buffer}");
                }
                    
            }
            return true.ToProperty();
        }

        [Property(QuietOnSuccess = true)]
        public Property CircularBuffer_size_is_always_accurate(CircularBuffer<int> buffer, int[] itemsToAdd)
        {
            buffer.Clear(); // found issues with old buffers being reused by FsCheck generators
            var currentSize = buffer.Size;
            if (currentSize > 0) return false.Label($"Began with invalid buffer size {currentSize}. Should be 0. Head: {buffer.Head}, Tail: {buffer.Tail}");
            for (var i = 0; i < itemsToAdd.Length; i++)
            {
                var full = currentSize == buffer.Capacity;
                var expectedSize = full ? currentSize : currentSize + 1;
                buffer.Enqueue(itemsToAdd[i]);
                var nextSize = buffer.Size;
                if (nextSize != expectedSize)
                    return false.When(buffer.Capacity > 0).Label($"Failed with {buffer} on operation {i+1}. Expected size to be {expectedSize}, was {nextSize}. Head: {buffer.Head}, Tail: {buffer.Tail}.");
                currentSize = nextSize;
            }
            return true.ToProperty();
        }

        [Property(QuietOnSuccess = true)]
        public Property CircularBuffer_Model_Should_Pass()
        {
            Func<int> generator = () => ThreadLocalRandom.Current.Next();
            var tests = new CircularBufferPropertyTests<int>(generator);
            return tests.ToProperty();
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
            return Gen.Elements(new Command<CircularBuffer<T>, IEnumerable<T>>[] { new Allocate(), new Enqueue(Generator), new Dequeue(), new Size(), });
        }

        public CircularBuffer<T> InitialActual => null; // no model yet - must be allocated as part of spec
        public IEnumerable<T> InitialModel => null; // no actual yet - must be allocated as part of spec

        private class Allocate : Command<CircularBuffer<T>, IEnumerable<T>>
        {
            private readonly Lazy<int> ListSize = new Lazy<int>(() => ThreadLocalRandom.Current.Next(1, 10));

            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {

                obj0 = new CircularBuffer<T>(ListSize.Value);
                return obj0;
            }

            public override IEnumerable<T> RunModel(IEnumerable<T> obj0)
            {
                obj0 = new List<T>();
                return obj0;
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
                return obj0.Concat(new[] { _data.Value }).ToList();
            }

            public override bool Pre(IEnumerable<T> _arg1)
            {
                return _arg1 != null;
            }

            public override Property Post(CircularBuffer<T> _arg2, IEnumerable<T> _arg3)
            {
                try
                {
                    var cbTail = _arg2.ToArray().Last();
                    var modelTail = _arg3.Last();

                    return
                        cbTail.Equals(modelTail)
                            .ToProperty()
                            .Label($"After enqueue expected Actual.Last()[{cbTail}] == Model.Last()[{modelTail}]");
                }
                catch (Exception ex)
                {
                    var foo = ex;
                    throw;
                }
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
