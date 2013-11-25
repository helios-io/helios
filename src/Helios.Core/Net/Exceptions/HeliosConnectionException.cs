using System;

namespace Helios.Core.Net.Exceptions
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
