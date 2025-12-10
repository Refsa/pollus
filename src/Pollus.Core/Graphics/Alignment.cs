namespace Pollus.Graphics;

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

public static class Alignment
{
    public static uint AlignedSize<T>(uint count)
        where T : unmanaged, IShaderType
    {
        return RoundUp(count * T.SizeOf, T.AlignOf);
    }

    public static uint AlignedSize<T>(uint count, uint alignment)
        where T : unmanaged
    {
        var size = (uint)Unsafe.SizeOf<T>() * count;
        return RoundUp(size, alignment);
    }

    public static bool IsAligned(uint size, uint alignment)
    {
        return (size % alignment) == 0;
    }

    public static uint RoundUp(uint size, uint alignment)
    {
        return (size + alignment - 1u) & ~(alignment - 1u);
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

    public static uint PreviousPowerOfTwo(this uint self)
    {
        if (self.IsPowerOfTwo()) return self;

        if (Lzcnt.IsSupported)
        {
            return 1u << (31 - (int)Lzcnt.LeadingZeroCount(self));
        }
        else
        {
            self |= self >> 1;
            self |= self >> 2;
            self |= self >> 4;
            self |= self >> 8;
            self |= self >> 16;
            return self - (self >> 1);
        }
    }

    public static uint ClosestPowerOfTwo(this uint self)
    {
        var next = self.NextPowerOfTwo();
        var prev = self.PreviousPowerOfTwo();

        if (next - self < self - prev)
        {
            return next;
        }

        return prev;
    }
}