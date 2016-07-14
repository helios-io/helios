// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Helios.Util
{
    public sealed class PoolHandle<T> where T : class
    {
        private readonly ObjectPool<T> _pool;

        internal PoolHandle(ObjectPool<T> pool)
        {
            _pool = pool;
        }

        public void Free(T obj)
        {
            _pool.Free(obj);
        }
    }

    /// <summary>
    ///     Pooling implementation for reusable objects
    ///     Roughly based on the Roslyn object pool implementation:
    ///     http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis/ObjectPool%25601.cs,991396bb82e6e4be
    /// </summary>
    public sealed class ObjectPool<T> where T : class
    {
        private readonly Func<PoolHandle<T>, T> _producer;
        private T _firstItem;
        private readonly Element[] _items;
        private readonly PoolHandle<T> _handle;

        public ObjectPool(Func<PoolHandle<T>, T> producer) : this(producer, Environment.ProcessorCount*2)
        {
        }

        public ObjectPool(Func<PoolHandle<T>, T> producer, int size)
        {
            Contract.Requires(producer != null);
            Contract.Requires(size >= 1);
            _handle = new PoolHandle<T>(this);
            _producer = producer;
            _items = new Element[size];
        }

        private T CreateInstance()
        {
            var obj = _producer(_handle); //separated the lines for debuggability
            return obj;
        }

        /// <summary>
        ///     Creates or reuses a pooled object instance.
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            var inst = _firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
            {
                inst = TakeSlow();
            }
            return inst;
        }

        private T TakeSlow()
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                var inst = items[i].Value;
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i].Value, null, inst))
                    {
                        return inst;
                    }
                }
            }

            return CreateInstance();
        }

        /// <summary>
        ///     Return an object to the pool
        /// </summary>
        public void Free(T obj)
        {
            Validate(obj);

            if (_firstItem == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                _firstItem = obj;
            }
            else
            {
                FreeSlow(obj);
            }
        }

        private void FreeSlow(T obj)
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i].Value = obj;
                    break;
                }
            }
        }

        [Conditional("DEBUG")]
        private void Validate(object obj)
        {
            Debug.Assert(obj != null, "Can't free null");
            Debug.Assert(_firstItem != obj, "can't free the same object twice!");
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].Value;
                if (value == null)
                {
                    return;
                }

                Debug.Assert(value != obj, "freeing the object twice!");
            }
        }

        private struct Element
        {
            internal T Value;
        }
    }
}