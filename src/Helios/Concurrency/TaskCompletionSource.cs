// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading.Tasks;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Non-generic implementation of a <see cref="TaskCompletionSource{TResult}" />
    /// </summary>
    public sealed class TaskCompletionSource : TaskCompletionSource<int>
    {
        public static readonly TaskCompletionSource Void = CreateVoidTcs();

        public TaskCompletionSource()
        {
        }

        public TaskCompletionSource(object state)
            : base(state)
        {
        }

        public bool TryComplete()
        {
            return TrySetResult(0);
        }

        public void Complete()
        {
            SetResult(0);
        }

        public bool SetUncancellable()
        {
            return true;
        }

        public override string ToString()
        {
            return "TaskCompletionSource[status: " + Task.Status + "]";
        }

        private static TaskCompletionSource CreateVoidTcs()
        {
            var tcs = new TaskCompletionSource();
            tcs.TryComplete();
            return tcs;
        }
    }
}