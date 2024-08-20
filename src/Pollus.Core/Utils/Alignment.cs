namespace Pollus.Utils;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

public static class Alignment
{
    public static uint GetAlignedSize<T>(bool findNextPowerOfTwo = true)
        where T : unmanaged
    {
        return GetAlignedSize((uint)Unsafe.SizeOf<T>(), findNextPowerOfTwo);
    }

    public static uint GetAlignedSize<T>(uint count, bool findNextPowerOfTwo = true)
        where T : unmanaged
    {
        return GetAlignedSize((uint)Unsafe.SizeOf<T>() * count, findNextPowerOfTwo);
    }

    public static uint GetAlignedSize(uint size, bool findNextPowerOfTwo = true)
    {
        if (size.IsPowerOfTwo() is false)
        {
            if (findNextPowerOfTwo)
            {
                return size.NextPowerOfTwo();
            }
            else
            {
                throw new ArgumentException("value needs to be a power of two", nameof(size));
            }
        }

        return size;
    }

    public static bool IsAligned(uint size, uint n)
    {
        return (size % n) == 0;
    }

    public static uint PaddingNeededFor(uint size, uint n)
    {
        return (n - (size % n)) % n;
    }

    public static uint RoundUp(uint size, uint n)
    {
        return n + PaddingNeededFor(size, n);
    }

    public static uint Max(Span<uint> alignments)
    {
        var max = alignments[0];

        for (int i = 1; i < alignments.Length; i++)
        {
            if (alignments[i] > max)
            {
                max = alignments[i];
            }
        }

        return max;
    }

    public static bool IsPowerOfTwo(this uint self)
    {
        return self != 0 && ((self & (self - 1)) == 0);
    }

    public static uint NextPowerOfTwo(this uint self)
    {
        if (Lzcnt.IsSupported)
        {
            return 1u << (32 - (int)Lzcnt.LeadingZeroCount(self - 1));
        }
        else
        {
            self--;
            self |= self >> 1;
            self |= self >> 2;
            self |= self >> 4;
            self |= self >> 8;
            self |= self >> 16;
            self++;
            return self;
        }
    }
}