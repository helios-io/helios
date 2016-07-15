// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;

namespace Helios.Buffers
{
    /// <summary>
    ///     Abstract base class for buffer implementation that wrap other <see cref="IByteBuf" />s internally,
    ///     such as <see cref="DuplicateByteBuf" /> and more.
    /// </summary>
    public abstract class AbstractDerivedByteBuffer : AbstractByteBuf
    {
        protected AbstractDerivedByteBuffer(int maxCapacity) : base(maxCapacity)
        {
        }

        public override int ReferenceCount => Unwrap().ReferenceCount;

        public override IReferenceCounted Retain()
        {
            Unwrap().Retain();
            return this;
        }

        public override IReferenceCounted Retain(int increment)
        {
            Unwrap().Retain(increment);
            return this;
        }

        public override IReferenceCounted Touch()
        {
            Unwrap().Touch();
            return this;
        }

        public override IReferenceCounted Touch(object hint)
        {
            Unwrap().Touch(hint);
            return this;
        }

        public override bool Release()
        {
            return Unwrap().Release();
        }

        public override bool Release(int decrement)
        {
            return Unwrap().Release(decrement);
        }

        public override ArraySegment<byte> GetIoBuffer(int index, int length) => Unwrap().GetIoBuffer(index, length);
    }
}