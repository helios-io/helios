using System.IO;

namespace Helios.Net
{
    /// <summary>
    /// Extension methods for working with NetworkData objects - deals primarily with Stream conversion
    /// </summary>
    public static class NetworkDataExtensions
    {
        public static Stream ToStream(this NetworkData nd)
        {
            return new MemoryStream(nd.Buffer, 0, nd.Length);
        }
    }
}