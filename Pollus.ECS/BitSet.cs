namespace Pollus.ECS;

using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public record struct BitSet256
{
    public long l0;
    public long l1;
    public long l2;
    public long l3;

    public BitSet256()
    {

    }

    public BitSet256(Span<int> indices)
    {
        foreach (var idx in indices)
        {
            Set(idx);
        }
    }

    public void Set(int idx)
    {
        switch (idx)
        {
            case < 64:
                l0 |= 1L << idx;
                break;
            case < 128:
                l1 |= 1L << idx - 64;
                break;
            case < 192:
                l2 |= 1L << idx - 128;
                break;
            default:
                l3 |= 1L << idx - 192;
                break;
        }
    }

    public void Set(Span<int> indices)
    {
        foreach (var idx in indices)
        {
            Set(idx);
        }
    }

    public void Unset(int idx)
    {
        switch (idx)
        {
            case < 64:
                l0 &= ~(1L << idx);
                break;
            case < 128:
                l1 &= ~(1L << idx - 64);
                break;
            case < 192:
                l2 &= ~(1L << idx - 128);
                break;
            default:
                l3 &= ~(1L << idx - 192);
                break;
        }
    }

    public bool Has(int idx)
    {
        return idx switch
        {
            < 64 => (l0 & 1L << idx) != 0,
            < 128 => (l1 & 1L << idx - 64) != 0,
            < 192 => (l2 & 1L << idx - 128) != 0,
            _ => (l3 & 1L << idx - 192) != 0
        };
    }

    public bool HasAll(BitSet256 other)
    {
        return (l0 & other.l0) == other.l0 &&
               (l1 & other.l1) == other.l1 &&
               (l2 & other.l2) == other.l2 &&
               (l3 & other.l3) == other.l3;
    }

    public bool HasAny(BitSet256 other)
    {
        return (l0 & other.l0) != 0 ||
               (l1 & other.l1) != 0 ||
               (l2 & other.l2) != 0 ||
               (l3 & other.l3) != 0;
    }

    public int FirstClearBit()
    {
        var b0 = BitOperations.TrailingZeroCount(~l0);
        if (b0 < 64) return b0;
        var b1 = BitOperations.TrailingZeroCount(~l1);
        if (b1 < 64) return b1 + 64;
        var b2 = BitOperations.TrailingZeroCount(~l2);
        if (b2 < 64) return b2 + 128;
        var b3 = BitOperations.TrailingZeroCount(~l3);
        if (b3 < 64) return b3 + 192;

        return -1;
    }
}