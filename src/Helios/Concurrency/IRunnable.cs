// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading.Tasks;

namespace Helios.Concurrency
{
    /// <summary>
    ///     Any aynchronous operation, whether it be a delegate, <see cref="Task" />, etc, which will
    ///     be executed later by an <see cref="IEventExecutor" />
    /// </summary>
    public interface IRunnable
    {
        void Run();
    }
}