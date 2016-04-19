using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Util
{
    /// <summary>
    /// Pooling implementation for reusable objects
    /// </summary>
    public sealed class ObjectPool<T> : IDisposable
    {
        abstract class ItemStore<T>
    }
}
