namespace Pollus.Tests.Collections;

using Pollus.Collections;

public class BitSetTests
{
    [Fact]
    public void Bitset256_Has()
    {
        var bitset = new BitSet256();
        bitset.Set(0);
        bitset.Set(64);
        bitset.Set(128);
        bitset.Set(192);

        Assert.True(bitset.Has(0));
        Assert.True(bitset.Has(64));
        Assert.True(bitset.Has(128));
        Assert.True(bitset.Has(192));
    }

    [Fact]
    public void Bitset256_HasAll()
    {
        var bitset = new BitSet256();
        bitset.Set(0);
        bitset.Set(64);
        bitset.Set(128);

        var other = new BitSet256();
        other.Set(0);
        other.Set(64);
        other.Set(128);

        Assert.True(bitset.HasAll(other));

        other.Set(192);
        Assert.False(bitset.HasAll(other));
    }

    [Fact]
    public void Bitset256_HasAny()
    {
        var bitset = new BitSet256();
        bitset.Set(0);
        bitset.Set(64);
        bitset.Set(128);

        var other = new BitSet256();
        other.Set(0);
        other.Set(64);
        other.Set(128);

        Assert.True(bitset.HasAny(other));

        other.Set(192);
        Assert.True(bitset.HasAny(other));

        bitset.Set(192);
        Assert.True(bitset.HasAny(other));
    }

    [Fact]
    public void Bitset256_FirstClearBit()
    {
        var bitset = new BitSet256();

        bitset.Set([0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        Assert.Equal(6, bitset.FirstClearBit());
        bitset.Set(6);
        Assert.Equal(16, bitset.FirstClearBit());
    }

    [Fact]
    public void Bitset256_FirstSetBit()
    {
        var bitset = new BitSet256();

        bitset.Set(192);
        Assert.Equal(192, bitset.FirstSetBit());

        bitset.Set(128);
        Assert.Equal(128, bitset.FirstSetBit());

        bitset.Set(64);
        Assert.Equal(64, bitset.FirstSetBit());

        bitset.Set([0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        Assert.Equal(0, bitset.FirstSetBit());
    }

    [Fact]
    public void Bitset256_LastSetBit()
    {
        var bitset = new BitSet256();

        bitset.Set([0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        Assert.Equal(15, bitset.LastSetBit());
        bitset.Unset(15);
        Assert.Equal(14, bitset.LastSetBit());

        bitset.Set(64);
        Assert.Equal(64, bitset.LastSetBit());

        bitset.Set(128);
        Assert.Equal(128, bitset.LastSetBit());

        bitset.Set(192);
        Assert.Equal(192, bitset.LastSetBit());
    }

    [Fact]
    public void Bitset256_Enumerator()
    {
        var bitset = new BitSet256();
        bitset.Set([1, 3, 5, 7, 9, 11, 13, 15]);

        var count = 0;
        foreach (var bit in bitset)
        {
            Assert.True(bit % 2 == 1);
            count++;
        }

        Assert.Equal(8, count);
    }

    [Fact]
    public void Bitset_Set()
    {
        var bitset = new BitSet(1);
        bitset.Set(0);
        bitset.Set(64);
        bitset.Set(128);
        bitset.Set(192);

        Assert.True(bitset.Has(0));
        Assert.True(bitset.Has(64));
        Assert.True(bitset.Has(128));
        Assert.True(bitset.Has(192));
    }

    [Fact]
    public void Bitset_Enumerator()
    {
        var bitset = new BitSet(1);
        Span<int> expected = stackalloc int[] { 1, 3, 5, 7, 70, 75, 140, 145, 210, 215, 280, 285, 350, 355, 420, 425, 1000 };
        bitset.Set(expected);

        var count = 0;
        foreach (var bit in bitset)
        {
            Assert.Equal(expected[count], bit);
            count++;
        }

        Assert.Equal(expected.Length, count);
    }

    [Fact]
    public void Bitset_Enumerator_Empty()
    {
        var bitset = new BitSet(1);
        var count = 0;
        foreach (var bit in bitset)
        {
            count++;
        }

        Assert.Equal(0, count);
    }
}