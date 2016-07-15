// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    internal sealed class ActionScheduledTask : ScheduledTask
    {
        private readonly Action _action;

        public ActionScheduledTask(AbstractScheduledEventExecutor executor, Action action, PreciseDeadline deadline)
            : base(executor, deadline, new TaskCompletionSource())
        {
            _action = action;
        }

        protected override void Execute()
        {
            _action();
        }
    }
}