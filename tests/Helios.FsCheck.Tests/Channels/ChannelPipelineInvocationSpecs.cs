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
        }

        public ChannelPipelineModel Model { get; }

        [Property(QuietOnSuccess = true, MaxTest = 10000, StartSize = 100)]
        public Property ChannelPipeline_should_obey_invocation_model()
        {
            return Model.ToProperty();
        }

        [Fact]
        public async Task ChannelPipeline_with_no_handlers_should_not_throw_on_invocation()
        {
            var pipeline = new DefaultChannelPipeline(TestChannel.Instance);
            await pipeline.BindAsync(null);
        }
    }
}
