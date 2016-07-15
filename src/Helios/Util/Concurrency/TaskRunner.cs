// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Util.Concurrency
{
    /// <summary>
    ///     Internal factory class for spawning Task instances
    /// </summary>
    internal static class TaskRunner
    {
        public const int DefaultConcurrency = 0;

        public static TaskFactory GetTaskFactory(int concurrencyLevel = DefaultConcurrency)
        {
            return new TaskFactory(TaskScheduler.Default);
        }

        public static Task Run(Action a)
        {
#if !NET35 && !NET40
            return Task.Run(a);
#else
            return Task.Factory.StartNew(a);
#endif
        }

        public static Task Run(Action a, CancellationToken c)
        {
#if !NET35 && !NET40
            return Task.Run(a, c);
#else
            return Task.Factory.StartNew(a, c);
#endif
        }

        public static Task<T> Run<T>(Func<T> f)
        {
#if !NET35 && !NET40
            return Task.Run(f);
#else
            return Task.Factory.StartNew(f);
#endif
        }

        public static Task<T> Run<T>(Func<T> f, CancellationToken c)
        {
#if !NET35 && !NET40
            return Task.Run(f, c);
#else
            return Task.Factory.StartNew(f,c);
#endif
        }

        public static Task Delay(TimeSpan gracePeriod)
        {
#if !NET35 && !NET40
            return Task.Delay(gracePeriod);
#else
            return Task.Factory.StartNew(() => Thread.Sleep(gracePeriod));
#endif
        }
    }
}