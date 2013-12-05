namespace Helios.Core.Util
{
    /// <summary>
    /// Static helper class for setting bitmask flags
    /// </summary>
    public static class FlagsHelper
    {
        public static bool IsSet<T>(T flags, T flag) where T : struct
        {
            var flagsValue = (int)(object)flags;
            var flagValue = (int)(object)flag;

            return (flagsValue & flagValue) != 0;
        }

        public static void Set<T>(ref T flags, T flag) where T : struct
        {
            var flagsValue = (int)(object)flags;
            var flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue | flagValue);
        }

        public static void Unset<T>(ref T flags, T flag) where T : struct
        {
            var flagsValue = (int)(object)flags;
            var flagValue = (int)(object)flag;

            flags = (T)(object)(flagsValue & (~flagValue));
        }
    }
}
