using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helios.Core.Connections.Exceptions
{
    public class HeliosConnectionException : Exception
    {
        private readonly ExceptionType _type;

        public HeliosConnectionException()
			: base()
		{
		}

        public HeliosConnectionException(ExceptionType type)
            : base()
        {
            this._type = type;
        }

        public HeliosConnectionException(ExceptionType type, string message) : base(message)
        {
            this._type = type;
        }

        public ExceptionType Type
        {
            get
            {
                return _type;
            }
        }
    }
}
