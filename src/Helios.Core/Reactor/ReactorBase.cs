using System;
using System.Net;

namespace Helios.Core.Reactor
{
    public abstract class ReactorBase : IReactor
    {
        public abstract bool IsActive { get; protected set; }
        public bool WasDisposed { get; protected set; }
        public abstract void Start();
        public abstract void Stop();
        public IPEndPoint LocalEndpoint { get; protected set; }

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