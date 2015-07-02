//-----------------------------------------------------------------------
// <copyright file="MonotonicClock.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Helios.Util
{
    /// <summary>
    /// Monotonic Clock - the right way to determine elapsed time, especially for performance-sensistive systems.
    /// 
    /// See https://www.softwariness.com/articles/monotonic-clocks-windows-and-posix/ for details.
    /// </summary>
    internal static class MonotonicClock
    {
        private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        private static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;
        [DllImport("kernel32")]
        private static extern ulong GetTickCount64();

        private const int TicksInMillisecond = 10000;

        /// <summary>
        /// Returns the total amount of elapsed time since the system started.
        /// 
        /// I.E. if this system has been up for 3 days, the underlying <see cref="TimeSpan"/>
        /// will be the equivalent of <code>TimeSpan.FromDays(3)</code>.
        /// </summary>
        public static TimeSpan Elapsed
        {
            get
            {
                return IsMono
                    ? Stopwatch.Elapsed
                    : new TimeSpan((long)GetTickCount64() * TicksInMillisecond);
            }
        }

        /// <summary>
        /// Returns the total amount of elapsed time since the system started, express as a <see cref="long"/>.
        /// 
        /// I.E. if this system has been up for 3 days, the underlying <see cref="TimeSpan"/>
        /// will be the equivalent of <code>TimeSpan.FromDays(3)</code>.
        /// </summary>
        public static long ElapsedTicks
        {
            get
            {
                return IsMono
                    ? Stopwatch.Elapsed.Ticks
                    : (long)GetTickCount64() * TicksInMillisecond;
            }
        }

        public static TimeSpan ElapsedHighRes
        {
            get { return Stopwatch.Elapsed; }
        }
    }
}
