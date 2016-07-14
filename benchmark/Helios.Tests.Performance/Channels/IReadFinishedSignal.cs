// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;
using Helios.Concurrency;

namespace Helios.Tests.Performance.Channels
{
    public interface IReadFinishedSignal
    {
        bool Finished { get; }
        void Signal();
    }

    public class ManualResetEventSlimReadFinishedSignal : IReadFinishedSignal
    {
        private readonly ManualResetEventSlim _manualResetEventSlim;

        public ManualResetEventSlimReadFinishedSignal(ManualResetEventSlim manualResetEventSlim)
        {
            _manualResetEventSlim = manualResetEventSlim;
        }

        public void Signal()
        {
            _manualResetEventSlim.Set();
        }

        public bool Finished => _manualResetEventSlim.IsSet;
    }

    public class TaskCompletionSourceFinishedSignal : IReadFinishedSignal
    {
        private readonly TaskCompletionSource _tcs;

        public TaskCompletionSourceFinishedSignal(TaskCompletionSource tcs)
        {
            _tcs = tcs;
        }

        public void Signal()
        {
            _tcs.TryComplete();
        }

        public bool Finished => _tcs.Task.IsCompleted;
    }

    public class SimpleReadFinishedSignal : IReadFinishedSignal
    {
        public void Signal()
        {
            Finished = true;
        }

        public bool Finished { get; private set; }
    }
}