using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels.Local
{
    public class LocalAddress : EndPoint
    {
        private readonly string id;

        /// <summary>
        /// Creates a new ephemeral port based on the ID of the specified <see cref="IChannel"/>
        /// </summary>
        /// <param name="channel"></param>
        public LocalAddress(IChannel channel)
        {
            
        }
    }
}
