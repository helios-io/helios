// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System.Collections.Generic;
using Helios.Buffers;
using Helios.Channels;
using Helios.Util;

namespace Helios.Codecs
{
    /// <summary>
    ///     Specialized <see cref="LengthFieldPrepender" /> that is designed to give Helios 2.0 the ability
    /// </summary>
    public class HeliosBackwardsCompatabilityLengthFramePrepender : LengthFieldPrepender
    {
        private readonly List<object> _temporaryOutput = new List<object>(2);

        public HeliosBackwardsCompatabilityLengthFramePrepender(int lengthFieldLength) : base(lengthFieldLength)
        {
        }

        public HeliosBackwardsCompatabilityLengthFramePrepender(int lengthFieldLength,
            bool lengthFieldIncludesLengthFieldLength) : base(lengthFieldLength, lengthFieldIncludesLengthFieldLength)
        {
        }

        public HeliosBackwardsCompatabilityLengthFramePrepender(int lengthFieldLength, int lengthAdjustment)
            : base(lengthFieldLength, lengthAdjustment)
        {
        }

        public HeliosBackwardsCompatabilityLengthFramePrepender(int lengthFieldLength, int lengthAdjustment,
            bool lengthFieldIncludesLengthFieldLength)
            : base(lengthFieldLength, lengthAdjustment, lengthFieldIncludesLengthFieldLength)
        {
        }

        public HeliosBackwardsCompatabilityLengthFramePrepender(ByteOrder byteOrder, int lengthFieldLength,
            int lengthAdjustment, bool lengthFieldIncludesLengthFieldLength)
            : base(byteOrder, lengthFieldLength, lengthAdjustment, lengthFieldIncludesLengthFieldLength)
        {
        }

        protected override void Encode(IChannelHandlerContext context, IByteBuf message, List<object> output)
        {
            base.Encode(context, message, _temporaryOutput);
            var lengthFrame = (IByteBuf) _temporaryOutput[0];
            var combined = lengthFrame.WriteBytes(message);
            ReferenceCountUtil.SafeRelease(message, 1); // ready to release it - bytes have been copied
            output.Add(combined.Retain());
            _temporaryOutput.Clear();
        }
    }
}