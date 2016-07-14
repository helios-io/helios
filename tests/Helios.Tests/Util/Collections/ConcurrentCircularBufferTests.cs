// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using Helios.Util.Collections;

namespace Helios.Tests.Util.Collections
{
    public class ConcurrentCircularBufferTests : CircularBufferTests
    {
        protected override ICircularBuffer<T> GetBuffer<T>(int capacity)
        {
            return new ConcurrentCircularBuffer<T>(capacity);
        }
    }
}