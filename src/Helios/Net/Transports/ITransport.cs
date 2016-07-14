// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;
using System.Threading.Tasks;

namespace Helios.Net.Transports
{
    /// <summary>
    ///     Interface used to place an I/O interface
    ///     on top of a connection
    /// </summary>
    public interface ITransport
    {
        bool Peek();

        int Read(byte[] buffer, int offset, int length);

        int ReadAll(byte[] buffer, int offset, int length);

        void Write(byte[] buffer);

        void Write(byte[] buffer, int offset, int length);

        void Flush();

#if !NET35 && !NET40

        Task<int> ReadAsync(byte[] buffer, int offset, int length);

        Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken token);
#endif

#if !NET35 && !NET40
        Task<int> ReadAllAsync(byte[] buffer, int offset, int length);

        Task<int> ReadAllAsync(byte[] buffer, int offset, int length, CancellationToken token);
#endif

#if !NET35 && !NET40
        Task WriteAsync(byte[] buffer);

        Task WriteAsync(byte[] buffer, CancellationToken token);

        Task WriteAsync(byte[] buffer, int offset, int length);

        Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken token);
#endif

#if !NET35 && !NET40
        Task FlushAsync();

        Task FlushAsync(CancellationToken token);
#endif
    }
}