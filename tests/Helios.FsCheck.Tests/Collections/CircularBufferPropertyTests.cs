// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.Xunit;
using Helios.Util;
using Helios.Util.Collections;

namespace Helios.FsCheck.Tests.Collections
{
    public class CircularBufferTests
    {
        public CircularBufferTests()
        {
            Arb.Register<HeliosGenerators>();
        }

        [Property]
        public Property CircularBuffer_dequeue_after_enqueue_should_return_original_item(CircularBuffer<int> buffer,
            int[] itemsToAdd)
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

        [Property]
        public Property CircularBuffer_size_is_always_accurate(CircularBuffer<int> buffer, int[] itemsToAdd)
        {
            buffer.Clear(); // found issues with old buffers being reused by FsCheck generators
            var currentSize = buffer.Size;
            if (currentSize > 0)
                return
                    false.Label(
                        $"Began with invalid buffer size {currentSize}. Should be 0. Head: {buffer.Head}, Tail: {buffer.Tail}");
            for (var i = 0; i < itemsToAdd.Length; i++)
            {
                var full = currentSize == buffer.Capacity;
                var expectedSize = full ? currentSize : currentSize + 1;
                buffer.Enqueue(itemsToAdd[i]);
                var nextSize = buffer.Size;
                if (nextSize != expectedSize)
                    return
                        false.When(buffer.Capacity > 0)
                            .Label(
                                $"Failed with {buffer} on operation {i + 1}. Expected size to be {expectedSize}, was {nextSize}. Head: {buffer.Head}, Tail: {buffer.Tail}.");
                currentSize = nextSize;
            }
            return true.ToProperty();
        }

        [Property(MaxTest = 1000)]
        public Property CircularBuffer_Model_Should_Pass()
        {
            Func<int> generator = () => ThreadLocalRandom.Current.Next();
            var tests = new CircularBufferPropertyTests<int>(generator, i => new CircularBuffer<int>(i));
            return tests.ToProperty();
        }

        [Property(MaxTest = 1000)]
        public Property ConcurrentCircularBuffer_Model_Should_Pass()
        {
            Func<int> generator = () => ThreadLocalRandom.Current.Next();
            var tests = new CircularBufferPropertyTests<int>(generator, i => new ConcurrentCircularBuffer<int>(i));
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

        public List<T> Items { get; }
        public int Capacity { get; }
    }

    public class CircularBufferPropertyTests<T> : ICommandGenerator<ICircularBuffer<T>, CModel<T>>

    {
        public CircularBufferPropertyTests(Func<T> generator, Func<int, ICircularBuffer<T>> bufferFactory)
        {
            Generator = generator;
            BufferFactory = bufferFactory;
        }

        public Func<T> Generator { get; }

        public Func<int, ICircularBuffer<T>> BufferFactory { get; }

        public Gen<Command<ICircularBuffer<T>, CModel<T>>> Next(CModel<T> obj0)
        {
            return
                Gen.Elements(new Command<ICircularBuffer<T>, CModel<T>>[]
                {
                    new Allocate(BufferFactory), new EnqueueNoWrapAround(Generator),
                    new EnqueueWithWrapAround(Generator),
                    new Dequeue(), new Size(), new Clear()
                });
        }

        public ICircularBuffer<T> InitialActual => null; // no model yet - must be allocated as part of spec
        public CModel<T> InitialModel => null; // no actual yet - must be allocated as part of spec

        private class Allocate : Command<ICircularBuffer<T>, CModel<T>>
        {
            private readonly Func<int, ICircularBuffer<T>> _factory;
            private readonly Lazy<int> _listSize = new Lazy<int>(() => ThreadLocalRandom.Current.Next(1, 10));

            public Allocate(Func<int, ICircularBuffer<T>> factory)
            {
                _factory = factory;
            }

            public override ICircularBuffer<T> RunActual(ICircularBuffer<T> obj0)
            {
                obj0 = _factory(_listSize.Value);
                return obj0;
            }

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0 = new CModel<T>(new List<T>(), _listSize.Value);
                return obj0;
            }

            public override string ToString()
            {
                return $"new CircularBuffer{typeof(T)}({_listSize.Value})";
            }

            public override bool Pre(CModel<T> _arg1)
            {
                return _arg1 == null;
            }
        }

        private class EnqueueNoWrapAround : Command<ICircularBuffer<T>, CModel<T>>
        {
            private readonly Lazy<T> _data;

            public EnqueueNoWrapAround(Func<T> generator)
            {
                Generator = generator;
                _data = new Lazy<T>(Generator);
            }

            private Func<T> Generator { get; }

            public override ICircularBuffer<T> RunActual(ICircularBuffer<T> obj0)
            {
                obj0.Enqueue(_data.Value);
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

            public override Property Post(ICircularBuffer<T> _arg2, CModel<T> _arg3)
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
                return $"CircularBuffer<{typeof(T)}>.EnqueueNoWrapAround({_data.Value})";
            }
        }

        private class EnqueueWithWrapAround : Command<ICircularBuffer<T>, CModel<T>>
        {
            private readonly Lazy<T> _data;

            public EnqueueWithWrapAround(Func<T> generator)
            {
                Generator = generator;
                _data = new Lazy<T>(Generator);
            }

            private Func<T> Generator { get; }

            public override ICircularBuffer<T> RunActual(ICircularBuffer<T> obj0)
            {
                obj0.Enqueue(_data.Value);
                return obj0;
            }

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0 = new CModel<T>(obj0.Items.Skip(1).Concat(new[] {_data.Value}).ToList(), obj0.Capacity);
                return obj0;
            }

            public override bool Pre(CModel<T> _arg1)
            {
                return _arg1 != null // must have called allocate first
                       && _arg1.Items.Any() // must have added at least 1 item to model
                       && _arg1.Items.Count == _arg1.Items.Capacity;
                // model size must equal capacity (forces wrap-around)
            }

            public override Property Post(ICircularBuffer<T> _arg2, CModel<T> _arg3)
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
                return $"CircularBuffer<{typeof(T)}>.EnqueueWithWrapAround({_data.Value})";
            }
        }

        private class Dequeue : Command<ICircularBuffer<T>, CModel<T>>
        {
            private T dequeValue;

            public override ICircularBuffer<T> RunActual(ICircularBuffer<T> obj0)
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

            public override Property Post(ICircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                return _arg2.Skip(1).SequenceEqual(_arg3.Items.Skip(1)).ToProperty();
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Dequeue() => {dequeValue}";
            }
        }

        private class Size : Command<ICircularBuffer<T>, CModel<T>>
        {
            public override ICircularBuffer<T> RunActual(ICircularBuffer<T> obj0)
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

            public override Property Post(ICircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                return
                    (_arg2.Count == _arg3.Items.Count).ToProperty()
                        .Label($"Expected {_arg3.Items.Count}, got {_arg2.Count}");
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Size()";
            }
        }

        private class Clear : Command<ICircularBuffer<T>, CModel<T>>
        {
            public override ICircularBuffer<T> RunActual(ICircularBuffer<T> obj0)
            {
                obj0.Clear();
                return obj0;
            }

            public override CModel<T> RunModel(CModel<T> obj0)
            {
                obj0.Items.Clear();
                return obj0;
            }

            public override bool Pre(CModel<T> _arg1)
            {
                return _arg1 != null;
            }

            public override Property Post(ICircularBuffer<T> _arg2, CModel<T> _arg3)
            {
                return
                    (_arg2.Count == _arg3.Items.Count).ToProperty()
                        .Label($"Expected {_arg3.Items.Count}, got {_arg2.Count}");
            }

            public override string ToString()
            {
                return $"CircularBuffer<{typeof(T)}>.Clear()";
            }
        }
    }
}