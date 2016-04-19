using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    public interface IChannel
    {
        IChannel Read();
    }
}
