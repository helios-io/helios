using System;

namespace Helios.Exceptions
{
    public class HeliosConnectionException : Exception
    {
        private readonly ExceptionType _type;

        public HeliosConnectionException()
			: this(ExceptionType.Unknown)
		{
		}

        public HeliosConnectionException(ExceptionType type)
            : base()
        {
            this._type = type;
        }

        public HeliosConnectionException(ExceptionType type, Exception innerException)
            : this(type, innerException.Message, innerException)
        {
        }

        public HeliosConnectionException(ExceptionType type, string message) : base(message)
        {
            this._type = type;
        }

        public HeliosConnectionException(ExceptionType type, string message, Exception innerException)
            : base(message, innerException)
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
