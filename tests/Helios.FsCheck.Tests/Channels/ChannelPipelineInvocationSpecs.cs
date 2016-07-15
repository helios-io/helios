// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FsCheck;
using FsCheck.Experimental;
using FsCheck.Xunit;
using Helios.Channels;
using Xunit;

namespace Helios.FsCheck.Tests.Channels
{
    /*
     * Models used for establshing the veracity of the pipeline invocation model.
     * 
     * Tests for correct ordering and chaining of invocations for all supported IChannelPipeline operations
     */

    public class ChannelPipelineInvocationSpecs
    {
        public ChannelPipelineInvocationSpecs()
        {
            Model = new ChannelPipelineModel(true);
            Arb.Register<AllEventsChannelHandler>();
        }

        public ChannelPipelineModel Model { get; }

        [Property]
        public Property ChannelPipeline_should_obey_invocation_model()
        {
            return Model.ToProperty();
        }

        [Property]
        public Property AllEventsChannelHandler_should_correctly_report_all_supported_events(SupportedEvent[] events)
        {
            var handler = new AllEventsChannelHandler("foo", events);
            return events.All(x => handler.SupportsEvent(x)).ToProperty();
        }

        [Property]
        public Property DefaultNamedChannelHandler_should_not_support_any_events(SupportedEvent[] events)
        {
            var handler = new NamedChannelHandler("foo");
            return events.All(x => !handler.SupportsEvent(x)).ToProperty();
        }

        [Fact]
        public async Task ChannelPipeline_with_no_handlers_should_not_throw_on_invocation()
        {
            var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
            await pipeline.BindAsync(null);
        }

        [Fact]
        public void ChannelPipeline_should_invoke_HandlerAdded_to_recently_added_handler()
        {
            var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
            var handler = new AllEventsChannelHandler("test", new[] {SupportedEvent.HandlerAdded});
            pipeline.AddFirst(handler.Name, handler);
            var head = ChannelPipelineModel.LastEventHistory(pipeline).Dequeue();
            Assert.Equal(handler.Name, head.Item1);
            Assert.Equal(SupportedEvent.HandlerAdded, head.Item2);
        }

        [Fact]
        public void ChannelPipeline_should_invoke_HandlerRemoved_to_removed_handler()
        {
            var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
            var handler = new AllEventsChannelHandler("test",
                new[] {SupportedEvent.HandlerAdded, SupportedEvent.HandlerRemoved});

            // add handler to pipeline first
            pipeline.AddFirst(handler.Name, handler);
            var head = ChannelPipelineModel.LastEventHistory(pipeline).Dequeue();

            // verify that handler added events fired correctly
            Assert.Equal(handler.Name, head.Item1);
            Assert.Equal(SupportedEvent.HandlerAdded, head.Item2);

            // remove handler from pipeline
            var removed = pipeline.RemoveFirst();
            Assert.Equal(handler, removed);

            // verify that handler removed event fired correctly
            var queue = new Queue<Tuple<string, SupportedEvent>>();
            ((NamedChannelHandler) removed).RecordLastFiredEvent(queue);
            Assert.Equal(SupportedEvent.HandlerRemoved, queue.Dequeue().Item2);
        }
    }
}