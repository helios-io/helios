using System.Linq;
using FsCheck;
using FsCheck.Experimental;
using FsCheck.Xunit;
using Helios.Channels;
using static Helios.FsCheck.Tests.Channels.ChannelPipelineModel;
using Xunit;

namespace Helios.FsCheck.Tests.Channels
{
    

    /// <summary>
    /// Test to validate that the <see cref="DefaultChannelPipeline"/> adds pipeline handlers
    /// to their correct and proper positions.
    /// </summary>
    public class ChannelPipelineConstructionSpecs
    {
        public ChannelPipelineConstructionSpecs()
        {
            Model = new ChannelPipelineModel();
        }

        public ChannelPipelineModel Model { get; }

        [Fact]
        public void ChannelPipeline_should_start_with_head_and_tail_handlers()
        {
            var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
            var count = pipeline.Count();
            Assert.Equal(2, count); // 1 for head, 1 for tail
        }

        [Fact]
        public void ChannelPipeline_should_add_item_to_head()
        {
            var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
            var namedChannel = new NamedChannelHandler("TEST");
            pipeline.AddFirst(namedChannel.Name, namedChannel);
            var count = pipeline.Count();
            Assert.Equal(3, count); // 1 for head, 1 for named channel, 1 for tail
        }

        [Fact]
        public void PipelineModel_should_detect_named_nodes_added_to_head()
        {
            var namedChannel = new NamedChannelHandler("TEST");
            var namedChannel2 = new NamedChannelHandler("TEST2");
            var node = new PipelineModelNode() {Handler = namedChannel, Name = namedChannel.Name};
            var pipelineModel = PipelineMutationModel.Fresh();
            pipelineModel = AddToHead(pipelineModel, node);
            Assert.True(pipelineModel.Contains(node.Name));
            var node2 = new PipelineModelNode() { Handler = namedChannel2, Name = namedChannel2.Name };
            pipelineModel = AddToHead(pipelineModel, node2);
            Assert.True(pipelineModel.Contains(node.Name));
            Assert.True(pipelineModel.Contains(node2.Name));
        }

        [Fact]
        public void PipelineModel_should_no_longer_detect_named_nodes_added_to_head()
        {
            var namedChannel = new NamedChannelHandler("TEST");
            var namedChannel2 = new NamedChannelHandler("TEST2");
            var node = new PipelineModelNode() { Handler = namedChannel, Name = namedChannel.Name };
            var pipelineModel = PipelineMutationModel.Fresh();
            pipelineModel = AddToHead(pipelineModel, node);
            Assert.True(pipelineModel.Contains(node.Name));
            pipelineModel = RemoveHead(pipelineModel);
            Assert.False(pipelineModel.Contains(namedChannel.Name));
        }

        [Fact]
        public void PipelineModel_should_detect_named_nodes_added_to_tail()
        {
            var namedChannel = new NamedChannelHandler("TEST");
            var namedChannel2 = new NamedChannelHandler("TEST2");
            var node = new PipelineModelNode() { Handler = namedChannel, Name = namedChannel.Name };
            var pipelineModel = PipelineMutationModel.Fresh();
            pipelineModel = AddToTail(pipelineModel, node);
            Assert.True(pipelineModel.Contains(node.Name));
            var node2 = new PipelineModelNode() { Handler = namedChannel2, Name = namedChannel2.Name };
            pipelineModel = AddToTail(pipelineModel, node2);
            Assert.True(pipelineModel.Contains(node.Name));
            Assert.True(pipelineModel.Contains(node2.Name));
        }

        [Fact]
        public void PipelineModel_should_no_longer_detect_named_nodes_removed_from_tail()
        {
            var namedChannel = new NamedChannelHandler("TEST");
            var node = new PipelineModelNode() { Handler = namedChannel, Name = namedChannel.Name };
            var pipelineModel = PipelineMutationModel.Fresh();
            pipelineModel = AddToTail(pipelineModel, node);
            Assert.True(pipelineModel.Contains(node.Name));
            pipelineModel = RemoveTail(pipelineModel);
            Assert.False(pipelineModel.Contains(namedChannel.Name));
        }

        //[Fact]
        //public void PipelineModel_handler_debug()
        //{
        //    var pipelineModel = PipelineMutationModel.Fresh();
        //    var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
        //    var namedChannel = new AllEventsChannelHandler("foo", new SupportedEvent[] { SupportedEvent.HandlerAdded });
        //    var node = new PipelineModelNode() { Handler = namedChannel, Name = namedChannel.Name };
          
        //    var addLast = new AddLast(namedChannel);
        //    addLast.Run(pipelineModel);
        //    var predictedOutcome = pipelineModel.PredictedOutcome(SupportedEvent.HandlerAdded);
        //    addLast.Check(pipeline, pipelineModel);
        //    var actualOutcome = EventQueue;
        //    Assert.Equal(predictedOutcome, actualOutcome);
        //}

        [Property(QuietOnSuccess = true, MaxTest = 1000)]
        public Property ChannelPipeline_should_obey_mutation_model()
        {
            return Model.ToProperty();
        }
    }
}
