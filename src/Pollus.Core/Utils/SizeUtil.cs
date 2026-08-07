using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pollus.Utils;

public static class SizeUtil
{
    public static uint SizeOf<T>(int length = 1)
        where T : unmanaged
    {
        return checked((uint)(Unsafe.SizeOf<T>() * length));
    }
}