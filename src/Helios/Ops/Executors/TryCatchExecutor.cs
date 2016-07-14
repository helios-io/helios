// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using Helios.Util;

namespace Helios.Ops.Executors
{
    public class TryCatchExecutor : BasicExecutor
    {
        private readonly Action<Exception> _exceptionCallback;

        public TryCatchExecutor() : this(exception => { })
        {
        }

        public TryCatchExecutor(Action<Exception> callback)
        {
            _exceptionCallback = callback;
        }


        public override void Execute(Action op)
        {
            try
            {
                if (!AcceptingJobs) return;
                op();
            }
            catch (Exception ex)
            {
                _exceptionCallback(ex);
            }
        }

        public override void Execute(IList<Action> ops, Action<IEnumerable<Action>> remainingOps)
        {
            var i = 0;
            try
            {
                for (; i < ops.Count; i++)
                {
                    if (!AcceptingJobs)
                    {
// ReSharper disable once AccessToModifiedClosure
                        remainingOps.NotNull(obj => remainingOps(ops.Skip(i + 1)));
                        break;
                    }

                    ops[i]();
                }
            }
            catch (Exception ex)
            {
                remainingOps.NotNull(obj => remainingOps(ops.Skip(i + 1)));
                _exceptionCallback(ex);
            }
        }
    }
}