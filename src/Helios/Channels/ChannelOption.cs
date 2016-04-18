using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    /// <summary>
    /// A strongly typed class representing a configuration option for a given channel.
    /// </summary>
    public abstract class ChannelOption
    {
        /// <summary>
        /// Sets the given value on the <see cref="IConnectionConfig"/>
        /// </summary>
        /// <param name="configuration">The underlying configuration object</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract bool Set(IConnectionConfig configuration, object value);
    }
}
