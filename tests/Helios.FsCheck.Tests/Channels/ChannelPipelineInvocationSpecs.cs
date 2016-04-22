using FsCheck;
using FsCheck.Experimental;
using FsCheck.Xunit;

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

        [Property(QuietOnSuccess = true, MaxTest = 1000)]
        public Property ChannelPipeline_should_obey_invocation_model()
        {
            return Model.ToProperty();
        }
    }
}
