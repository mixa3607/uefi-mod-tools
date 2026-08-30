using System.Runtime.InteropServices;

namespace ArkProjects.UefiModTools.Utils;

public static class MarshalHelper
{
    public static T FromBytes<T>(ReadOnlySpan<byte> span) where T : struct
    {
        var data = span.ToArray();

        var size = Marshal.SizeOf<T>();
        if (size > data.Length)
            throw new ArgumentException($"Require {size} bytes at least");

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(data, 0, ptr, size);
#pragma warning disable IL2091
            return Marshal.PtrToStructure<T>(ptr);
#pragma warning restore IL2091
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
