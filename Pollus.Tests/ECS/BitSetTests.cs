namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class BitSetTests
{
    [Fact]
    public void bitset_Has()
    {
        var bitset = new BitSet();
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
    public void bitset_HasAll()
    {
        var bitset = new BitSet();
        bitset.Set(0);
        bitset.Set(64);
        bitset.Set(128);

        var other = new BitSet();
        other.Set(0);
        other.Set(64);
        other.Set(128);

        Assert.True(bitset.HasAll(other));

        other.Set(192);
        Assert.False(bitset.HasAll(other));
    }

    [Fact]
    public void bitset_HasAny()
    {
        var bitset = new BitSet();
        bitset.Set(0);
        bitset.Set(64);
        bitset.Set(128);

        var other = new BitSet();
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
    public void bitset_FirstClearBit()
    {
        var bitset = new BitSet();

        bitset.Set([0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        Assert.Equal(6, bitset.FirstClearBit());
        bitset.Set(6);
        Assert.Equal(16, bitset.FirstClearBit());
    }
}