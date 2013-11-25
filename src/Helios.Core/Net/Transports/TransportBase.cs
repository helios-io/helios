using System;
using System.Threading;
using System.Threading.Tasks;
using Helios.Core.Net.Exceptions;
using Helios.Core.Util.Concurrency;

namespace Helios.Core.Net.Transports
{
    public abstract class TransportBase : ITransport
    {
        public abstract bool IsOpen();
        public abstract void Open();
        public abstract void Close();

        public bool Peek()
        {
            return IsOpen();
        }

        public abstract int Read(byte[] buffer, int offset, int length);
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int length);
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken token);

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

        public async Task<int> ReadAllAsync(byte[] buffer, int offset, int length)
        {
            return await TaskRunner.Run(() => ReadAll(buffer, offset, length));
        }

        public async Task<int> ReadAllAsync(byte[] buffer, int offset, int length, CancellationToken token)
        {
            return await TaskRunner.Run(() => ReadAll(buffer, offset, length), token);
        }

        public virtual void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public abstract Task WriteAsync(byte[] buffer);
        public abstract Task WriteAsync(byte[] buffer, CancellationToken token);

        public abstract void Write(byte[] buffer, int offset, int length);
        public abstract Task WriteAsync(byte[] buffer, int offset, int length);
        public abstract Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken token);

        public virtual void Flush()
        {
        }

        public abstract Task FlushAsync();
        public abstract Task FlushAsync(CancellationToken token);

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);

        #endregion
    }
}