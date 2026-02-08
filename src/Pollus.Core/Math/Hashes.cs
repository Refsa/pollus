using System.Runtime.CompilerServices;

namespace Pollus.Mathematics;

public static class Hashes
{
    // https://nullprogram.com/blog/2018/07/31/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint LowBias32(uint n)
    {
        n ^= n >> 16;
        n *= 0x7feb352dU;
        n ^= n >> 15;
        n *= 0x846ca68bU;
        n ^= n >> 16;
        return n;
    }

    // https://nullprogram.com/blog/2018/07/31/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint InverseLowBias32(uint x)
    {
        x ^= x >> 16;
        x *= 0x43021123U;
        x ^= x >> 15 ^ x >> 30;
        x *= 0x1d69e2a5U;
        x ^= x >> 16;
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Hash(int x)
    {
        return ToInt(Hash(ToUInt(x)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Hash(uint x)
    {
        x = ((x >> 16) ^ x) * 0x45d9f3b;
        x = ((x >> 16) ^ x) * 0x45d9f3b;
        x = (x >> 16) ^ x;
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint HashInverse(uint x)
    {
        x = ((x >> 16) ^ x) * 0x119de1f3;
        x = ((x >> 16) ^ x) * 0x119de1f3;
        x = (x >> 16) ^ x;
        return x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(uint value)
    {
        return (int)value ^ -2147483648;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUInt(int value)
    {
        long temp = value + 2147483648;
        return (uint)temp;
    }

    public static int Mix(int key)
    {
        key ^= key >> 16;
        key *= unchecked((int)0x85ebca6b);
        key ^= key >> 13;
        key *= unchecked((int)0xc2b2ae35);
        key ^= key >> 16;
        return key;
    }

    public static int ZobristHash(int id)
    {
        unchecked
        {
            long x = id + (long)0x9e3779b97f4a7c15L;
            x = (x ^ (x >> 30)) * (long)0xbf58476d1ce4e5b9L;
            x = (x ^ (x >> 27)) * (long)0x94d049bb133111ebL;
            return (int)(x ^ (x >> 31));
        }
    }
}