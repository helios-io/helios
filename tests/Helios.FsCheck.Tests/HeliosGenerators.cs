using System;
using FsCheck;
using Helios.Util;
using Helios.Util.Collections;
using Helios.Logging;

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
