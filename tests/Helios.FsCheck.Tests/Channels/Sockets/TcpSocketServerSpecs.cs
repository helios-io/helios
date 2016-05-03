using FsCheck;
using FsCheck.Experimental;
using FsCheck.Xunit;
using Xunit;

namespace Helios.FsCheck.Tests.Channels.Sockets
{
    public class TcpSocketServerSpecs
    {
        [Fact(Skip = "XUnit hates our threads :(")]
        public void TcpSeverSocketChannel_should_obey_model()
        {
            var model = new TcpServerSocketChannelStateMachine();
            try
            {
                model.ToProperty().VerboseCheckThrowOnFailure();
            }
            finally
            {
                model.ShutdownAll();
            }
        }
    }
}
