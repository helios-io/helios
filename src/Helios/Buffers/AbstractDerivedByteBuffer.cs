// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

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
            return Unwrap().Retain();
        }

        public override IReferenceCounted Retain(int increment)
        {
            return Unwrap().Retain(increment);
        }

        public override IReferenceCounted Touch()
        {
            return Unwrap().Touch();
        }

        public override IReferenceCounted Touch(object hint)
        {
            return Unwrap().Touch(hint);
        }

        public override bool Release()
        {
            return Unwrap().Release();
        }

        public override bool Release(int decrement)
        {
            return Unwrap().Release(decrement);
        }
    }
}

