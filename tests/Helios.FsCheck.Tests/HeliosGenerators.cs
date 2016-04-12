using System;
using FsCheck;
using Helios.Util;
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
    }
}
