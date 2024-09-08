namespace Pollus.Collections;

using System.Numerics;
using System.Runtime.CompilerServices;

/// <summary>
/// Non allocating 256 bit wide bitset
/// </summary>
public record struct BitSet256
{
    public ulong l0;
    public ulong l1;
    public ulong l2;
    public ulong l3;

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
                l0 |= 1UL << idx;
                break;
            case < 128:
                l1 |= 1UL << idx - 64;
                break;
            case < 192:
                l2 |= 1UL << idx - 128;
                break;
            default:
                l3 |= 1UL << idx - 192;
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
                l0 &= ~(1UL << idx);
                break;
            case < 128:
                l1 &= ~(1UL << idx - 64);
                break;
            case < 192:
                l2 &= ~(1UL << idx - 128);
                break;
            default:
                l3 &= ~(1UL << idx - 192);
                break;
        }
    }

    public bool Has(int idx)
    {
        return idx switch
        {
            < 64 => (l0 & 1UL << idx) != 0,
            < 128 => (l1 & 1UL << idx - 64) != 0,
            < 192 => (l2 & 1UL << idx - 128) != 0,
            _ => (l3 & 1UL << idx - 192) != 0
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

    public int FirstSetBit()
    {
        var b0 = BitOperations.TrailingZeroCount(l0);
        if (b0 < 64) return b0;
        var b1 = BitOperations.TrailingZeroCount(l1);
        if (b1 < 64) return b1 + 64;
        var b2 = BitOperations.TrailingZeroCount(l2);
        if (b2 < 64) return b2 + 128;
        var b3 = BitOperations.TrailingZeroCount(l3);
        if (b3 < 64) return b3 + 192;
        return -1;
    }

    public int LastSetBit()
    {
        var b3 = BitOperations.LeadingZeroCount(l3);
        if (b3 < 64) return 255 - b3;
        var b2 = BitOperations.LeadingZeroCount(l2);
        if (b2 < 64) return 191 - b2;
        var b1 = BitOperations.LeadingZeroCount(l1);
        if (b1 < 64) return 127 - b1;
        var b0 = BitOperations.LeadingZeroCount(l0);
        if (b0 < 64) return 63 - b0;
        return -1;
    }

    public Enumerator GetEnumerator()
    {
        return new(this);
    }

    public ref struct Enumerator
    {
        BitSet256 bitset;
        int current;
        int end;

        public Enumerator(BitSet256 bitset)
        {
            this.bitset = bitset;
            current = bitset.FirstSetBit() - 1;
            end = bitset.LastSetBit();
        }

        public int Current => current;

        public bool MoveNext()
        {
            if (end == -1) return false;

            while (++current <= end)
            {
                if (bitset.Has(current))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

/// <summary>
/// growing dense bitset
/// </summary>
public record struct BitSet : IDisposable
{
    NativeArray<ulong> data;

    public BitSet(int bitcount = 1)
    {
        bitcount = Math.Max(1, bitcount);
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
            Set(idx);
            return;
        }
        data[bucket] |= 1UL << idx % 64;
    }

    public void Set(ReadOnlySpan<int> indices)
    {
        foreach (var idx in indices)
        {
            Set(idx);
        }
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

    public int LastSetBit()
    {
        for (int i = data.Length - 1; i >= 0; i--)
        {
            var b = BitOperations.LeadingZeroCount(data[i]);
            if (b < 64)
            {
                return i * 64 + 63 - b;
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
        Unsafe.CopyBlock(newData.Data, data.Data, (uint)data.Length * sizeof(ulong));
        data.Dispose();
        data = newData;
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        BitSet bitset;
        int blockIdx;
        int bitIdx;
        int lastIdx;

        public Enumerator(BitSet bitset)
        {
            this.bitset = bitset;
            blockIdx = -1;
        }

        public int Current => blockIdx * 64 + bitIdx;

        public bool MoveNext()
        {
            if (bitIdx == lastIdx)
            {
                while (++blockIdx < bitset.data.Length && bitset.data[blockIdx] == 0);
                if (blockIdx >= bitset.data.Length) return false;
                lastIdx = LastSetBit(bitset.data[blockIdx]);
                bitIdx = FirstSetBit(bitset.data[blockIdx]) - 1;
            }

            while (++bitIdx < lastIdx)
            {
                if (HasBit(bitset.data[blockIdx], bitIdx))
                {
                    return true;
                }
            }

            return true;
        }

        bool HasBit(ulong value, int idx)
        {
            return (value & 1UL << idx) != 0;
        }

        int FirstSetBit(ulong value)
        {
            return BitOperations.TrailingZeroCount(value);
        }

        int LastSetBit(ulong value)
        {
            return 63 - BitOperations.LeadingZeroCount(value);
        }
    }
}