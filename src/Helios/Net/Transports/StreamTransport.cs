using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Helios.Exceptions;

namespace Helios.Net.Transports
{
    public class StreamTransport : TransportBase, IStreamTransport
    {
        public StreamTransport() { }

        public StreamTransport(Stream inputStream, Stream outputStream)
        {
            InputStream = inputStream;
            OutputStream = outputStream;
        }

        public void CloseStreams()
        {
            if (InputStream != null)
            {
                InputStream.Close();
                InputStream = null;
            }

            if (OutputStream != null)
            {
                OutputStream.Close();
                OutputStream = null;
            }
        }

        #region Reads

        public override bool Peek()
        {
            return true;
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            CheckInputStream();
            return InputStream.Read(buffer, offset, length);
        }

#if !NET35 && !NET40
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            CheckInputStream();
            return await InputStream.ReadAsync(buffer, offset, length);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken token)
        {
            CheckInputStream();
            return await InputStream.ReadAsync(buffer, offset, length, token);
        }
#endif

        #endregion

        #region Writes

#if !NET35 && !NET40
        public override async Task WriteAsync(byte[] buffer)
        {
            CheckOutputStream();
            await OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        public override async Task WriteAsync(byte[] buffer, CancellationToken token)
        {
            CheckOutputStream();
            await OutputStream.WriteAsync(buffer, 0, buffer.Length, token);
        }
#endif

        public override void Write(byte[] buffer, int offset, int length)
        {
            CheckOutputStream();
            OutputStream.Write(buffer, offset, length);
        }

#if !NET35 && !NET40
        public override async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            CheckOutputStream();
            await OutputStream.WriteAsync(buffer, offset, length);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken token)
        {
            CheckOutputStream();
            await OutputStream.WriteAsync(buffer, offset, length, token);
        }
#endif

        #endregion

        public override void Flush()
        {
            CheckOutputStream();
            OutputStream.Flush();
        }

#if !NET35 && !NET40
        public override async Task FlushAsync()
        {
            CheckOutputStream();
            await OutputStream.FlushAsync();
        }

        public override async Task FlushAsync(CancellationToken token)
        {
            CheckOutputStream();
            await OutputStream.FlushAsync(token);
        }
#endif

        public Stream OutputStream { get; protected set; }
        public Stream InputStream { get; protected set; }

        #region IDisposable Members

        private bool _isDisposed;

        public virtual void DisposeStreams(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (InputStream != null)
                        InputStream.Dispose();
                    if (OutputStream != null)
                        OutputStream.Dispose();
                }
            }
            _isDisposed = true;
        }

        #endregion

        #region Null Checks

        private void CheckInputStream()
        {
            if (InputStream == null)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Stream is not open for reading");
            }
        }

        private void CheckOutputStream()
        {
            if (OutputStream == null)
            {
                throw new HeliosConnectionException(ExceptionType.NotOpen, "Stream is not open for writing");
            }
        }

        #endregion
    }
}