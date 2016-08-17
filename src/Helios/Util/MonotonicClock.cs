// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Helios.Util
{
    internal static class MonotonicClock
    {
        private const int TicksInMillisecond = 10000;

        private const long NanosPerTick = 100;
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        internal static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

        public static TimeSpan Elapsed
        {
            get { return TimeSpan.FromTicks(GetTicks()); }
        }

        public static TimeSpan ElapsedHighRes
        {
            get { return Stopwatch.Elapsed; }
        }

        [DllImport("kernel32")]
        private static extern ulong GetTickCount64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetMilliseconds()
        {
            return IsMono
                ? Stopwatch.ElapsedMilliseconds
                : (long) GetTickCount64();
        }

        public static long GetNanos()
        {
            return GetTicks()*NanosPerTick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTicks()
        {
            return GetMilliseconds()*TicksInMillisecond;
        }

        /// <summary>
        ///     Ticks represent 100 nanos. https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
        ///     This extension method converts a Ticks value to nano seconds.
        /// </summary>
        internal static long ToNanos(this long ticks)
        {
            return ticks*NanosPerTick;
        }

        /// <summary>
        ///     Ticks represent 100 nanos. https://msdn.microsoft.com/en-us/library/system.datetime.ticks(v=vs.110).aspx
        ///     This extension method converts a nano seconds value to Ticks.
        /// </summary>
        internal static long ToTicks(this long nanos)
        {
            return nanos/NanosPerTick;
        }
    }
}