// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using FsCheck;
using Helios.Logging;
using Helios.Util.Collections;

namespace Helios.FsCheck.Tests
{
    public class HeliosGenerators
    {
        public static Arbitrary<CircularBuffer<T>> CreateCircularBuffer<T>()
        {
            var generator = Gen.Choose(1, 10).Select(i => new CircularBuffer<T>(i));
            return Arb.From(generator);
        }

        public static Arbitrary<CircularBuffer<int>> CreateCircularBufferInt()
        {
            return CreateCircularBuffer<int>();
        }

        public static Arbitrary<LogLevel> CreateLogLevel()
        {
            return Arb.From(Gen.Elements(LogLevel.Error, LogLevel.Debug, LogLevel.Info, LogLevel.Warning));
        }
    }
}

