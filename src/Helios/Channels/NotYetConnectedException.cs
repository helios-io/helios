using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    public class NotYetConnectedException : IOException
    {
        private NotYetConnectedException() { }

        public static readonly NotYetConnectedException Instance = new NotYetConnectedException();
    }
}
