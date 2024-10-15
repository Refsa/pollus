namespace Pollus.Utils;

using System.Runtime.InteropServices;

public static class RandomUtils
{
    public static string RandomString(this System.Random random, int length)
    {
        Span<byte> result = stackalloc byte[length];
        for (int i = 0; i < length / 8; i++)
        {
            MemoryMarshal.Write(result.Slice(i * 8), Random.Shared.NextInt64(long.MinValue, long.MaxValue));
        }

        if (length % 8 != 0)
        {
            MemoryMarshal.Write(result.Slice(length / 8 * 8), Random.Shared.NextInt64(long.MinValue, long.MaxValue));
        }

        return new string(MemoryMarshal.Cast<byte, char>(result));
    }
}