using System.Security.Cryptography;

namespace ArkProjects.UefiModTools.Utils;

public static class ByteArrayExtensions
{
    extension(ReadOnlySpan<byte> bytes)
    {
        public string GetSha256String()
        {
            return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }
    }

    extension(byte[] bytes)
    {
        public string GetSha256String()
        {
            return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }
    }
}
