// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal abstract class ScheduledAsyncTask : ScheduledTask
    {
        private static readonly Action<object> CancellationAction = s => ((ScheduledAsyncTask) s).Cancel();
        private readonly CancellationToken _cancellationToken;
        private CancellationTokenRegistration _cancellationTokenRegistration;

        protected ScheduledAsyncTask(AbstractScheduledEventExecutor executor, PreciseDeadline deadline,
            TaskCompletionSource promise, CancellationToken cancellationToken)
            : base(executor, deadline, promise)
        {
            _cancellationToken = cancellationToken;
            _cancellationTokenRegistration = cancellationToken.Register(CancellationAction, this);
        }

        public override void Run()
        {
            _cancellationTokenRegistration.Dispose();
            if (_cancellationToken.IsCancellationRequested)
            {
                Promise.TrySetCanceled();
            }
            else
            {
                base.Run();
            }
        }
    }
}