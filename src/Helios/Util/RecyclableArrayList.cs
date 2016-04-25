using System.Collections.Generic;
using System.Threading;

namespace Helios.Util
{
    public class RecyclableArrayList : List<object>
    {
        static readonly ThreadLocal<ObjectPool<RecyclableArrayList>> _pool = new ThreadLocal<ObjectPool<RecyclableArrayList>>(() => new ObjectPool<RecyclableArrayList>(() => new RecyclableArrayList()));

        public static RecyclableArrayList Take()
        {
            return _pool.Value.Take();
        }

        public void Return()
        {
            Clear();
            _pool.Value.Free(this); // return to the pool
        }
    }
}