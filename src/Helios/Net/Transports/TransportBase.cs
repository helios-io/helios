// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Threading;
using System.Threading.Tasks;
using Helios.Exceptions;
using Helios.Util.Concurrency;

namespace Helios.Net.Transports
{
    public abstract class TransportBase : ITransport
    {
        public abstract bool Peek();
        public abstract int Read(byte[] buffer, int offset, int length);

        public int ReadAll(byte[] buffer, int offset, int length)
        {
            var totalRead = 0;
            var read = 0;

            while (totalRead < length)
            {
                read = Read(buffer, offset + totalRead, length - totalRead);
                if (read <= 0)
                {
                    throw new HeliosConnectionException(ExceptionType.EndOfFile,
                        "Cannot read - remote side closed unexpectedly");
                }
                totalRead += read;
            }

            return totalRead;
        }

        public virtual void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }


        public abstract void Write(byte[] buffer, int offset, int length);


        public virtual void Flush()
        {
        }

#if !NET35 && !NET40
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int length);
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken token);
#endif

#if !NET35 && !NET40
        public async Task<int> ReadAllAsync(byte[] buffer, int offset, int length)
        {
            return await TaskRunner.Run(() => ReadAll(buffer, offset, length));
        }

        public async Task<int> ReadAllAsync(byte[] buffer, int offset, int length, CancellationToken token)
        {
            return await TaskRunner.Run(() => ReadAll(buffer, offset, length), token);
        }
#endif

#if !NET35 && !NET40
        public abstract Task WriteAsync(byte[] buffer);
        public abstract Task WriteAsync(byte[] buffer, CancellationToken token);

        public abstract Task WriteAsync(byte[] buffer, int offset, int length);
        public abstract Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken token);
#endif

#if !NET35 && !NET40
        public abstract Task FlushAsync();
        public abstract Task FlushAsync(CancellationToken token);
#endif
    }
}