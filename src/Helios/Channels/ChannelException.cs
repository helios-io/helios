using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helios.Exceptions;

namespace Helios.Channels
{
    public class HeliosChannelPipelineException : Exception
    {
        public HeliosChannelPipelineException(string message, Exception cause = null) : base(message, cause) { }
    }
}
