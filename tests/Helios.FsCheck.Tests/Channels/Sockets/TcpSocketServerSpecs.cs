using FsCheck;
using FsCheck.Experimental;
using FsCheck.Xunit;
using Xunit;

namespace Helios.FsCheck.Tests.Channels.Sockets
{
    public class TcpSocketServerSpecs
    {
        [Fact(Skip = "Resolved XUnit issue, but need to disable shrinking")]
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
