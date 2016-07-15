// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal sealed class ActionScheduledAsyncTask : ScheduledAsyncTask
    {
        private readonly Action _action;

        public ActionScheduledAsyncTask(AbstractScheduledEventExecutor executor, Action action, PreciseDeadline deadline,
            CancellationToken cancellationToken)
            : base(executor, deadline, new TaskCompletionSource(), cancellationToken)
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action();
        }
    }
}