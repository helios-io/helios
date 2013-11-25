using System;
using System.Threading;
using System.Threading.Tasks;

namespace Helios.Core.Net.Transports
{
    /// <summary>
    /// Interface used to place an I/O interface
    /// on top of a connection
    /// </summary>
    public interface ITransport : IDisposable
    {
        bool IsOpen();

        void Open();

        void Close();

        bool Peek();

        int Read(byte[] buffer, int offset, int length);

        Task<int> ReadAsync(byte[] buffer, int offset, int length);

        Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken token);

        int ReadAll(byte[] buffer, int offset, int length);

        Task<int> ReadAllAsync(byte[] buffer, int offset, int length);

        Task<int> ReadAllAsync(byte[] buffer, int offset, int length, CancellationToken token);

        void Write(byte[] buffer);

        Task WriteAsync(byte[] buffer);

        Task WriteAsync(byte[] buffer, CancellationToken token);

        void Write(byte[] buffer, int offset, int length);

        Task WriteAsync(byte[] buffer, int offset, int length);

        Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken token);

        void Flush();

        Task FlushAsync();

        Task FlushAsync(CancellationToken token);

        void Dispose(bool disposing);
    }
}
