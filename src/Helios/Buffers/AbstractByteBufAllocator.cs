// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Buffers
{
    /// <summary>
    ///     Abstract base class for <see cref="IByteBufAllocator" /> instances
    /// </summary>
    public abstract class AbstractByteBufAllocator : IByteBufAllocator
    {
        private readonly IByteBuf _emptyBuf;

        protected AbstractByteBufAllocator()
        {
            _emptyBuf = new EmptyByteBuf(this);
        }

        public IByteBuf Buffer()
        {
            return DirectBuffer();
        }

        public IByteBuf Buffer(int initialCapcity)
        {
            return DirectBuffer(initialCapcity);
        }

        public IByteBuf Buffer(int initialCapacity, int maxCapacity)
        {
            return DirectBuffer(initialCapacity, maxCapacity);
        }

        #region Range validation

        private static void Validate(int initialCapacity, int maxCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException("initialCapacity", "initialCapacity must be greater than zero");

            if (initialCapacity > maxCapacity)
                throw new ArgumentOutOfRangeException("initialCapacity",
                    string.Format("initialCapacity ({0}) must be greater than maxCapacity ({1})", initialCapacity,
                        maxCapacity));
        }

        #endregion

        #region Direct buffer methods

        protected IByteBuf DirectBuffer()
        {
            return DirectBuffer(ByteBufferUtil.DEFAULT_INITIAL_CAPACITY, int.MaxValue);
        }

        protected IByteBuf DirectBuffer(int initialCapacity)
        {
            return DirectBuffer(initialCapacity, int.MaxValue);
        }

        protected IByteBuf DirectBuffer(int initialCapacity, int maxCapacity)
        {
            if (initialCapacity == 0 && maxCapacity == 0)
                return _emptyBuf;

            Validate(initialCapacity, maxCapacity);

            return NewDirectBuffer(initialCapacity, maxCapacity);
        }

        protected abstract IByteBuf NewDirectBuffer(int initialCapacity, int maxCapacity);

        #endregion
    }
}