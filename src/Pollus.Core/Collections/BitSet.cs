namespace Pollus.Collections;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Non allocating 256 bit wide bitset
/// </summary>
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

/// <summary>
/// growing bitset
/// </summary>
public record struct BitSet : IDisposable
{
    NativeArray<ulong> data;

    public BitSet(int bitcount)
    {
        data = new(bitcount / 64 + 1);
    }

    public void Dispose()
    {
        data.Dispose();
    }

    public int GetCount()
    {
        int count = 0;
        for (int i = 0; i < data.Length; i++)
        {
            count += BitOperations.PopCount(data[i]);
        }
        return count;
    }

    public void Set(int idx)
    {
        int bucket = idx / 64;
        if (bucket >= data.Length)
        {
            Resize();
        }
        data[bucket] |= 1UL << idx % 64;
    }

    public void Unset(int idx)
    {
        int bucket = idx / 64;
        if (bucket >= data.Length) return;
        data[bucket] &= ~(1UL << idx % 64);
    }

    public bool Has(int idx)
    {
        int bucket = idx / 64;
        if (bucket >= data.Length) return false;
        return (data[bucket] & 1UL << idx % 64) != 0;
    }

    public bool HasAll(BitSet other)
    {
        if (data.Length != other.data.Length) return false;

        for (int i = 0; i < data.Length; i++)
        {
            if ((data[i] & other.data[i]) != other.data[i])
            {
                return false;
            }
        }
        return true;
    }

    public bool HasAny(BitSet other)
    {
        for (int i = 0; i < int.Min(data.Length, other.data.Length); i++)
        {
            if ((data[i] & other.data[i]) != 0)
            {
                return true;
            }
        }
        return false;
    }

    public int FirstClearBit()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var b = BitOperations.TrailingZeroCount(~data[i]);
            if (b < 64)
            {
                return i * 64 + b;
            }
        }
        return -1;
    }

    public int FirstSetBit()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var b = BitOperations.TrailingZeroCount(data[i]);
            if (b < 64)
            {
                return i * 64 + b;
            }
        }
        return -1;
    }

    public void Clear()
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }

    unsafe void Resize()
    {
        var newData = new NativeArray<ulong>(data.Length * 2);
        Unsafe.CopyBlock(data.Data, newData.Data, (uint)data.Length * sizeof(ulong));
        data.Dispose();
        data = newData;
    }
}