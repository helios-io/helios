using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Channels
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SkipAttribute : Attribute
    {
    }
}
