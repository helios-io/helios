// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using Helios.Buffers;
using Helios.Logging;

namespace Helios.Util
{
    public sealed class ReferenceCountUtil
    {
        private static readonly ILogger Logger = LoggingFactory.GetLogger<ReferenceCountUtil>();

        /// <summary>
        ///     Try to call {@link ReferenceCounted#retain()} if the specified message implements {@link ReferenceCounted}.
        ///     If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
        /// </summary>
        public static T Retain<T>(T msg)
        {
            var counted = msg as IReferenceCounted;
            if (counted != null)
            {
                return (T) counted.Retain();
            }
            return msg;
        }

        /// <summary>
        ///     Try to call {@link ReferenceCounted#retain(int)} if the specified message implements {@link ReferenceCounted}.
        ///     If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
        /// </summary>
        public static T Retain<T>(T msg, int increment)
        {
            var counted = msg as IReferenceCounted;
            if (counted != null)
            {
                return (T) counted.Retain(increment);
            }
            return msg;
        }

        /// <summary>
        ///     Tries to call <see cref="IReferenceCounted.Touch()" /> if the specified message implements
        ///     <see cref="IReferenceCounted" />.
        ///     If the specified message doesn't implement <see cref="IReferenceCounted" />, this method does nothing.
        /// </summary>
        public static T Touch<T>(T msg)
        {
            var refCnt = msg as IReferenceCounted;
            if (refCnt != null)
            {
                return (T) refCnt.Touch();
            }
            return msg;
        }

        /// <summary>
        ///     Tries to call <see cref="IReferenceCounted.Touch(object)" /> if the specified message implements
        ///     <see cref="IReferenceCounted" />. If the specified message doesn't implement <see cref="IReferenceCounted" />,
        ///     this method does nothing.
        /// </summary>
        public static T Touch<T>(T msg, object hint)
        {
            var refCnt = msg as IReferenceCounted;
            if (refCnt != null)
            {
                return (T) refCnt.Touch(hint);
            }
            return msg;
        }

        /// <summary>
        ///     Try to call {@link ReferenceCounted#release()} if the specified message implements {@link ReferenceCounted}.
        ///     If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
        /// </summary>
        public static bool Release(object msg)
        {
            var counted = msg as IReferenceCounted;
            if (counted != null)
            {
                return counted.Release();
            }
            return false;
        }

        /// <summary>
        ///     Try to call {@link ReferenceCounted#release(int)} if the specified message implements {@link ReferenceCounted}.
        ///     If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
        /// </summary>
        public static bool Release(object msg, int decrement)
        {
            var counted = msg as IReferenceCounted;
            if (counted != null)
            {
                return counted.Release(decrement);
            }
            return false;
        }

        /// <summary>
        ///     Try to call {@link ReferenceCounted#release()} if the specified message implements {@link ReferenceCounted}.
        ///     If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
        ///     Unlike {@link #release(Object)} this method catches an exception raised by {@link ReferenceCounted#release()}
        ///     and logs it, rather than rethrowing it to the caller.  It is usually recommended to use {@link #release(Object)}
        ///     instead, unless you absolutely need to swallow an exception.
        /// </summary>
        public static void SafeRelease(object msg)
        {
            try
            {
                Release(msg);
            }
            catch (Exception ex)
            {
                Logger.Warning("Failed to release a message: {}", msg, ex);
            }
        }

        /// <summary>
        ///     Try to call {@link ReferenceCounted#release(int)} if the specified message implements {@link ReferenceCounted}.
        ///     If the specified message doesn't implement {@link ReferenceCounted}, this method does nothing.
        ///     Unlike {@link #release(Object)} this method catches an exception raised by {@link ReferenceCounted#release(int)}
        ///     and logs it, rather than rethrowing it to the caller.  It is usually recommended to use
        ///     {@link #release(Object, int)} instead, unless you absolutely need to swallow an exception.
        /// </summary>
        public static void SafeRelease(object msg, int decrement)
        {
            try
            {
                Release(msg, decrement);
            }
            catch (Exception ex)
            {
                if (Logger.IsWarningEnabled)
                {
                    Logger.Warning(ex, "Failed to release a message: {0} (decrement: {1})", msg, decrement);
                }
            }
        }
    }
}