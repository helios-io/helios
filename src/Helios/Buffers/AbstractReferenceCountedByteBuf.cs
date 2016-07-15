// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Diagnostics.Contracts;
using System.Threading;

namespace Helios.Buffers
{
    /// <summary>
    ///     An abstract <see cref="IByteBuf" /> implementation that uses reference counting to protect the underlying
    ///     data from being overwritten in scenarios where buffers are re-used, sliced, or otherwise shared.
    /// </summary>
    public abstract class AbstractReferenceCountedByteBuf : AbstractByteBuf
    {
        private volatile int _refCount = 1;

        protected AbstractReferenceCountedByteBuf(int maxCapacity) : base(maxCapacity)
        {
        }

        public override int ReferenceCount => _refCount;

        /// <summary>
        ///     An unsafe operation designed to be used by a subclass that sets the reference count of the buffer directly
        /// </summary>
        /// <param name="refCount">The new <see cref="ReferenceCount" /> value to use.</param>
        protected void SetReferenceCount(int refCount)
        {
            _refCount = refCount;
        }

        public override IReferenceCounted Retain()
        {
            while (true)
            {
                var refCount = _refCount;
                if (refCount == 0)
                    throw new IllegalReferenceCountException(0, 1);
                if (refCount == int.MaxValue)
                    throw new IllegalReferenceCountException(int.MaxValue, 1);

                if (Interlocked.CompareExchange(ref _refCount, refCount + 1, refCount) == refCount)
                    break;
            }
            return this;
        }

        public override IReferenceCounted Retain(int increment)
        {
            Contract.Requires(increment > 0);

            while (true)
            {
                var refCount = _refCount;
                if (refCount == 0)
                    throw new IllegalReferenceCountException(0, increment);
                if (refCount > int.MaxValue - increment)
                    throw new IllegalReferenceCountException(refCount, increment);

                if (Interlocked.CompareExchange(ref _refCount, refCount + increment, refCount) == refCount)
                    break;
            }
            return this;
        }

        public override IReferenceCounted Touch()
        {
            return this;
        }

        public override IReferenceCounted Touch(object hint)
        {
            return this;
        }

        public override bool Release()
        {
            while (true)
            {
                var refCount = _refCount;
                if (refCount == 0)
                    throw new IllegalReferenceCountException(0, -1);

                if (Interlocked.CompareExchange(ref _refCount, refCount - 1, refCount) == refCount)
                {
                    if (refCount == 1)
                    {
                        Deallocate();
                        return true;
                    }
                    return false;
                }
            }
        }

        public override bool Release(int decrement)
        {
            Contract.Requires(decrement > 0);

            while (true)
            {
                var refCount = _refCount;
                if (refCount < decrement)
                    throw new IllegalReferenceCountException(refCount, decrement);

                if (Interlocked.CompareExchange(ref _refCount, refCount - decrement, refCount) == refCount)
                {
                    if (refCount == decrement)
                    {
                        Deallocate();
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        ///     Called once <see cref="ReferenceCount" /> is equal to <c>0</c>.
        /// </summary>
        protected abstract void Deallocate();
    }
}