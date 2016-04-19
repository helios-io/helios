using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    public class ClosedChannelException : IOException
    {
        private ClosedChannelException() { }

        public static readonly ClosedChannelException Instance = new ClosedChannelException();
    }
}
