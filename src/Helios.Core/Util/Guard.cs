using System;

namespace Helios.Core.Util
{
    /// <summary>
    /// Guard class for protecting against stupid input
    /// </summary>
    public static class Guard
    {
        public static void NotNegative(this int value)
        {
            NotLessThan(value, 0);
        }

        public static void NotLessThan(this int value, int minimumValue)
        {
            if(value < minimumValue)
                throw new ArgumentOutOfRangeException("value", string.Format("Value was {0} - cannot be less than {1}!", value, minimumValue));
        }

        public static void NotGreaterThan(this int value, int maximumValue)
        {
            if(value > maximumValue)
                throw new ArgumentOutOfRangeException("value", string.Format("Value was {0} - cannot be greater than {1}!", value, maximumValue));
        }

        public static void True(bool boolean, string errorMessage = "Expression should be true, but was false")
        {
            if(!boolean)
                throw new ArgumentException(errorMessage);
        }
    }
}
