// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using Helios.Buffers;
using Helios.Channels;
using Helios.Util;

namespace Helios.Codecs
{
    /// <summary>
    /// Used to decode one message type into another
    /// </summary>
    /// <remarks>
    /// Be aware that you need to call <see cref="IReferenceCounted.Retain()"/> on messages that are just passed through 
    /// if they are of type <see cref="IReferenceCounted"/>. This is needed as the <see cref="MessageToMessageDecoder{TMessage}"/> 
    /// will call <see cref="IReferenceCounted.Release()"/> on decoded messages.
    /// </remarks>
    public abstract class MessageToMessageDecoder<TMessage> : ChannelHandlerAdapter
    {
        public bool AcceptInboundMessage(object msg)
        {
            return msg is TMessage;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var output = RecyclableArrayList.Take();
            try
            {
                if (AcceptInboundMessage(message))
                {
                    var cast = (TMessage) message;
                    try
                    {
                        Decode(context, cast, output);
                    }
                    finally
                    {
                        ReferenceCountUtil.Release(cast);
                    }
                }
                else
                {
                    output.Add(message);
                }
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DecoderException(ex);
            }
            finally
            {
                var size = output.Count;
                for (var i = 0; i < size; i++)
                {
                    context.FireChannelRead(output[i]);
                }
                output.Return();
            }
        }

        protected abstract void Decode(IChannelHandlerContext context, TMessage message, List<object> output);
    }
}