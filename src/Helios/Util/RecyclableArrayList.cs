// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using System.Threading;

namespace Helios.Util
{
    public class RecyclableArrayList : List<object>
    {
        private static readonly ThreadLocal<ObjectPool<RecyclableArrayList>> _pool =
            new ThreadLocal<ObjectPool<RecyclableArrayList>>(
                () => new ObjectPool<RecyclableArrayList>(handle => new RecyclableArrayList(handle)));

        private readonly PoolHandle<RecyclableArrayList> _handle;

        public RecyclableArrayList(PoolHandle<RecyclableArrayList> handle)
        {
            _handle = handle;
        }

        public static RecyclableArrayList Take()
        {
            return _pool.Value.Take();
        }

        public void Return()
        {
            Clear();
            _handle.Free(this); // return to the pool
        }
    }
}