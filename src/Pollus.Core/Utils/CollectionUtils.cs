namespace Pollus.Utils;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class CollectionUtils
{
    public static T[] Distinct<T>(params ReadOnlySpan<T[]> arrays)
    {
        var set = new HashSet<T>();
        foreach (var array in arrays)
        {
            foreach (var item in array)
            {
                set.Add(item);
            }
        }

        return set.ToArray();
    }

    public static byte[] GetBytes<T>(in T value)
        where T : unmanaged
    {
        var bytes = new byte[Unsafe.SizeOf<T>()];
        MemoryMarshal.Write(bytes.AsSpan(), value);
        return bytes;
    }
}