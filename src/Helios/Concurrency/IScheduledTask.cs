// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Helios.Util.TimedOps;

namespace Helios.Concurrency
{
    public interface IScheduledTask
    {
        PreciseDeadline Deadline { get; }

        Task Completion { get; }
        bool Cancel();

        TaskAwaiter GetAwaiter();
    }
}