using System;

namespace Helios.ServiceStore
{
    /// <summary>
    /// A static helper class for protecting against pesky null reference errors
    /// </summary>
    public static class NullGuard
    {
        public static TOut NotNull<TIn, TOut>(this TIn obj, Func<TIn, TOut> nextOp) where TOut:class
                                                                                 where TIn:class
        {
            if (obj == null)
                return default(TOut);
            return nextOp.Invoke(obj);
        }

        public static void NotNull<TIn>(this TIn obj, Action<TIn> nextOp) where TIn : class
        {
            if (obj == null)
                return;
            nextOp.Invoke(obj);
        }
    }
}
