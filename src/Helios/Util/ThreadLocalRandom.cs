// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;

namespace Helios.Util
{
    /// <summary>
    ///     Create random numbers with Thread-specific seeds.
    ///     Borrowed form Jon Skeet's brilliant C# in Depth: http://csharpindepth.com/Articles/Chapter12/Random.aspx
    /// </summary>
    public static class ThreadLocalRandom
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> _rng =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        /// <summary>
        ///     The current random number seed available to this thread
        /// </summary>
        public static Random Current
        {
            get { return _rng.Value; }
        }
    }
}