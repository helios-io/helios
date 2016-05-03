using System;
using Microsoft.FSharp.Core;

namespace Helios.FsCheck.Tests
{
    public static class FsharpDelegateHelper
    {
        public static FSharpFunc<T2, TResult> Create<T2, TResult>(Func<T2, TResult> func)
        {
            Converter<T2, TResult> conv = input => func(input);
            return FSharpFunc<T2, TResult>.FromConverter(conv);
        }

        public static FSharpFunc<T1, FSharpFunc<T2, TResult>> Create<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            Converter<T1, FSharpFunc<T2, TResult>> conv = value1 =>
            {
                return Create<T2, TResult>(value2 => func(value1, value2));
            };
            return FSharpFunc<T1, FSharpFunc<T2, TResult>>.FromConverter(conv);
        }
    }
}