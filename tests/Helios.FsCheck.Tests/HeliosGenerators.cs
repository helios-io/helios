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
            Func<CircularBuffer<T>> generator = () =>
            {
                var randomSize = ThreadLocalRandom.Current.Next(1, 10);
                var buffer = new CircularBuffer<T>(randomSize);
                return buffer;
            };

            return Arb.From(Gen.Fresh(generator));
        }

        public static Arbitrary<CircularBuffer<int>> CreateCircularBufferInt()
        {
            return CreateCircularBuffer<int>();
        }
    }
}
