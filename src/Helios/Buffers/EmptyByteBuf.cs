// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Util;

namespace Helios.Buffers
{
    /// <summary>
    ///     Represents an empty byte buffer
    /// </summary>
    public class EmptyByteBuf : AbstractByteBuf
    {
        static readonly ArraySegment<byte> EmptyBuffer = new ArraySegment<byte>(ByteArrayExtensions.Empty);
        static readonly ArraySegment<byte>[] EmptyBuffers = {EmptyBuffer};

        public EmptyByteBuf(IByteBufAllocator allocator) : base(0)
        {
            Allocator = allocator;
        }

        public override int Capacity
        {
            get { return 0; }
        }

        public override ByteOrder Order => ByteOrder.LittleEndian;

        public override IByteBufAllocator Allocator { get; }

        public override bool HasArray
        {
            get { return false; }
        }

        public override byte[] Array
        {
            get { throw new NotSupportedException(); }
        }

        public override bool IsDirect
        {
            get { return true; }
        }

        public override int ArrayOffset => 0;

        public override int ReferenceCount
        {
            get { return 1; }
        }

        public override int IoBufferCount => 1;

        public override ArraySegment<byte> GetIoBuffer(int index, int length)
        {
            CheckIndex(index, length);
            return EmptyBuffer;
        }

        public override ArraySegment<byte>[] GetIoBuffers(int index, int length)
        {
            CheckIndex(index, length);
            return GetIoBuffers();
        }

        public override IByteBuf AdjustCapacity(int newCapacity)
        {
            throw new NotSupportedException();
        }

        protected override byte _GetByte(int index)
        {
            throw new IndexOutOfRangeException();
        }

        protected override short _GetShort(int index)
        {
            throw new IndexOutOfRangeException();
        }

        protected override int _GetInt(int index)
        {
            throw new IndexOutOfRangeException();
        }

        protected override long _GetLong(int index)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf GetBytes(int index, IByteBuf destination, int dstIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf GetBytes(int index, byte[] destination, int dstIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetByte(int index, int value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetShort(int index, int value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetInt(int index, int value)
        {
            throw new IndexOutOfRangeException();
        }

        protected override IByteBuf _SetLong(int index, long value)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf SetBytes(int index, IByteBuf src, int srcIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf SetBytes(int index, byte[] src, int srcIndex, int length)
        {
            throw new IndexOutOfRangeException();
        }

        public override IByteBuf Copy(int index, int length)
        {
            return this;
        }

        public override IByteBuf Unwrap()
        {
            return null;
        }

        public override IByteBuf Compact()
        {
            return this;
        }

        public override IByteBuf CompactIfNecessary()
        {
            return this;
        }

        public override IReferenceCounted Retain()
        {
            return this;
        }

        public override IReferenceCounted Retain(int increment)
        {
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
            return false;
        }

        public override bool Release(int decrement)
        {
            return false;
        }
    }
}