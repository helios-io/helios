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
                    return false.When(buffer.Capacity > 0).Label($"Failed with {buffer} on operation {i + 1}. Expected size to be {expectedSize}, was {nextSize}. Head: {buffer.Head}, Tail: {buffer.Tail}.");
                currentSize = nextSize;
            }
            return true.ToProperty();
        }

        [Property(QuietOnSuccess = true, MaxTest = 1000)]
        public Property CircularBuffer_Model_Should_Pass()
        {
            Func<int> generator = () => ThreadLocalRandom.Current.Next();
            var tests = new CircularBufferPropertyTests<int>(generator);
            return tests.ToProperty();
        }
    }

    public class CModel<T>
    {
        public CModel(List<T> items, int capacity)
        {
            Items = items;
            Capacity = capacity;
        }

        public List<T> Items { get; private set; }
        public int Capacity { get; private set; }
    }

    public class CircularBufferPropertyTests<T> : ICommandGenerator<CircularBuffer<T>, CModel<T>>

    {


        public Func<T> Generator { get; }

        public CircularBufferPropertyTests(Func<T> generator)
        {
            Generator = generator;
        }

        public Gen<Command<CircularBuffer<T>, CModel<T>>> Next(CModel<T> obj0)
        {
            return Gen.Elements(new Command<CircularBuffer<T>, CModel<T>>[] { new Allocate(), new EnqueueNoWrapAround(Generator), new Dequeue(), new Size(), });
        }

        public CircularBuffer<T> InitialActual => null; // no model yet - must be allocated as part of spec
        public CModel<T> InitialModel => null; // no actual yet - must be allocated as part of spec

        private class Allocate : Command<CircularBuffer<T>, CModel<T>>
        {
            private readonly Lazy<int> ListSize = new Lazy<int>(() => ThreadLocalRandom.Current.Next(1, 10));

            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {

                obj0 = new CircularBuffer<T>(ListSize.Value);
                return obj0;
            }

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0 = new CModel<T>(new List<T>(), ListSize.Value);
                return obj0;
            }

            public override string ToString()
            {
                return $"new CircularBuffer{typeof(T)}({ListSize.Value})";
            }

            public override bool Pre(CModel<T> _arg1)
            {
                return _arg1 == null;
            }
        }

        private class EnqueueNoWrapAround : Command<CircularBuffer<T>, CModel<T>>
        {
            public EnqueueNoWrapAround(Func<T> generator)
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

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0 = new CModel<T>(obj0.Items.Concat(new[] {_data.Value}).ToList(), obj0.Capacity);
                return obj0;
            }

            public override bool Pre(CModel<T> _arg1)
            {
                // ensure no wrap-around
                return _arg1 != null && _arg1.Items.Count < _arg1.Items.Capacity;
            }

            public override Property Post(CircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                var cbTail = _arg2.ToArray().Last();
                var modelTail = _arg3.Items.Last();

                return
                    cbTail.Equals(modelTail)
                        .ToProperty()
                        .Label($"After enqueue expected Actual.Last()[{cbTail}] == Model.Last()[{modelTail}]");
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Enqueue({_data.Value})";
            }
        }

        private class EnqueueWithWrapAround : Command<CircularBuffer<T>, CModel<T>>
        {
            public EnqueueWithWrapAround(Func<T> generator)
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

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0 = new CModel<T>(obj0.Items.Skip(1).Concat(new[] { _data.Value }).ToList(), obj0.Capacity);
                return obj0;
            }

            public override bool Pre(CModel<T> _arg1)
            {
                // ensure no wrap-around
                return _arg1 != null && _arg1.Items.Count == _arg1.Items.Capacity;
            }

            public override Property Post(CircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                var cbTail = _arg2.ToArray().Last();
                var modelTail = _arg3.Items.Last();

                return
                    cbTail.Equals(modelTail)
                        .ToProperty()
                        .Label($"After enqueue expected Actual.Last()[{cbTail}] == Model.Last()[{modelTail}]");
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Enqueue({_data.Value})";
            }
        }

        private class Dequeue : Command<CircularBuffer<T>, CModel<T>>
        {
            private T dequeValue;

            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {
                dequeValue = obj0.Dequeue();
                return obj0;
            }

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0 = new CModel<T>(obj0.Items.Skip(1).ToList(), obj0.Capacity);
                return obj0;
            }

            public override bool Pre(CModel<T> _arg1)
            {
                return _arg1 != null && _arg1.Items.Any();
            }

            public override Property Post(CircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                return (_arg2.Skip(1).SequenceEqual(_arg3.Items.Skip(1))).ToProperty();
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Dequeue() => {dequeValue}";
            }
        }

        private class Size : Command<CircularBuffer<T>, CModel<T>>
        {
            public override CircularBuffer<T> RunActual(CircularBuffer<T> obj0)
            {
                return obj0;
            }

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                return obj0;
            }

            public override bool Pre(CModel<T> _arg1)
            {
                return _arg1 != null;
            }

            public override Property Post(CircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                return (_arg2.Count == _arg3.Items.Count).ToProperty().Label($"Expected {_arg3.Items.Count}, got {_arg2.Count}");
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Size()";
            }
        }
    }
}
