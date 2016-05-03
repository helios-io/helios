using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helios.Buffers;
using Helios.Channels;
using Helios.Concurrency;
using Helios.Util;
using Helios.Util.Concurrency;

namespace Helios.Codecs
{
    /// <summary>
    /// <see cref="IChannelHandler"/> implementation which encodes from one type of message to another.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MessageToMessageEncoder<T> : ChannelHandlerAdapter
    {
        public bool CanAcceptOutboundMessage(object message)
        {
            return message is T;
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            Task result = null;
            RecyclableArrayList output = null;
            try
            {
                if (CanAcceptOutboundMessage(message))
                {
                    T cast = (T)message;
                    output = RecyclableArrayList.Take();
                    try
                    {
                        Encode(context, cast, output);
                    }
                    finally
                    {
                        // TODO: reference counting
                    }

                    if (!output.Any())
                    {
                        output.Return();
                        output = null;

                        throw new EncoderException($"{GetType()} must produce at least one message");
                    }
                }
                else
                {
                    return context.WriteAsync(message);
                }
            }
            catch (EncoderException ex)
            {
                return TaskEx.FromException(ex);
            }
            catch (Exception ex)
            {
                return TaskEx.FromException(new EncoderException(ex));
            }
            finally
            {
                if (output != null)
                {
                    var sizeMinusOne = output.Count - 1;
                    if (sizeMinusOne == 0)
                    {
                        result = context.WriteAsync(output[0]);
                    }
                    else if (sizeMinusOne > 0)
                    {
                        // TODO: netty does some promise optimizations here, which our API doesn't support at the moment
                        for (var i = 0; i < sizeMinusOne; i++)
                        {
                            context.WriteAsync(output[i]);
                        }
                        result = context.WriteAsync(output[sizeMinusOne]);
                    }

                    output.Return();
                }
            }
            return result;
        }

        public void Write(IChannelHandlerContext context, object message)
        {
           
        }

        protected abstract void Encode(IChannelHandlerContext context, T cast, List<object> output);
    }
}
