// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Threading;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal sealed class ActionWithStateScheduledAsyncTask : ScheduledAsyncTask
    {
        private readonly Action<object> _action;

        public ActionWithStateScheduledAsyncTask(AbstractScheduledEventExecutor executor, Action<object> action,
            object state, PreciseDeadline deadline, CancellationToken cancellationToken)
            : base(executor, deadline, new TaskCompletionSource(state), cancellationToken)
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action(Completion.AsyncState);
        }
    }
}