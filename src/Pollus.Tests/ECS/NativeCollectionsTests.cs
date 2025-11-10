namespace Pollus.Tests.Collections;

using Pollus.Collections;

public class NativeCollectionsTests
{
    [Fact]
    public void NativeMap_Test()
    {
        using var map = new NativeMap<int, int>(0);

        map.Add(1, 10);
        map.Add(2, 20);
        map.Add(3, 30);

        Assert.True(map.Has(1));
        Assert.True(map.Has(2));
        Assert.True(map.Has(3));

        Assert.False(map.Has(4));
        Assert.False(map.Has(100));
        Assert.False(map.Has(10000));

        Assert.Equal(10, map.Get(1));
        Assert.Equal(20, map.Get(2));
        Assert.Equal(30, map.Get(3));
    }

    [Fact]
    public void NativeMap_SparseRemove()
    {
        using var map = new NativeMap<int, int>(32);

        for (int i = 0; i < 32; i++)
        {
            map.Add(i, i);
        }

        for (int i = 8; i < 24; i++)
        {
            map.Remove(i);
        }

        map.Add(84, 84);
        map.Add(52, 52);
        map.Remove(84);

        Assert.True(map.Has(52));
    }

    [Fact]
    public void NativeMap_NegativeKey()
    {
        using var map = new NativeMap<int, int>(0);

        map.Add(-1, 10);
        map.Add(-2, 20);
        map.Add(-3, 30);

        Assert.True(map.Has(-1));
        Assert.True(map.Has(-2));
        Assert.True(map.Has(-3));

        Assert.False(map.Has(-4));
        Assert.False(map.Has(-100));
        Assert.False(map.Has(-10000));

        Assert.Equal(10, map.Get(-1));
        Assert.Equal(20, map.Get(-2));
        Assert.Equal(30, map.Get(-3));
    }

    [Fact]
    public void NativeMap_Large()
    {
        using var map = new NativeMap<int, int>(0);
        for (int i = 0; i < 1_000_000; i++)
        {
            map.Add(i, i);
        }

        for (int i = 0; i < 1_000_000; i++)
        {
            Assert.True(map.Has(i));
            Assert.Equal(i, map.Get(i));
        }
    }

    [Fact]
    public void NativeMap_Large_Negative_Sparse()
    {
        using var map = new NativeMap<int, int>(0);
        for (int i = 0; i > -100_000_000; i -= 100)
        {
            map.Add(i, i);
        }

        for (int i = 0; i > -100_000_000; i -= 100)
        {
            Assert.True(map.Has(i), $"Key {i} not found");
            Assert.Equal(i, map.Get(i));
        }
    }

    [Fact]
    public void NativeMap_Large_Negative_Dense()
    {
        using var map = new NativeMap<int, int>(0);
        for (int i = 0; i > -1_000_000; i--)
        {
            map.Add(i, i);
        }

        for (int i = 0; i > -1_000_000; i--)
        {
            Assert.True(map.Has(i), $"Key {i} not found");
            Assert.Equal(i, map.Get(i));
        }
    }

    [Fact]
    public void NativeMap_KeyEnumerator()
    {
        using var map = new NativeMap<int, int>(0);
        for (int i = 0; i < 1_000; i += 2)
        {
            map.Add(i, i);
        }

        int idx = 0;
        int count = 0;
        foreach (var key in map.Keys)
        {
            Assert.Equal(idx * 2, key);
            count++;
            idx++;
        }
        Assert.Equal(500, count);
    }

    [Fact]
    public void NativeMap_ValueEnumerator()
    {
        using var map = new NativeMap<int, int>(0);
        for (int i = 0; i < 1_000; i += 2)
        {
            map.Add(i, i);
        }

        int idx = 0;
        int count = 0;
        foreach (var value in map.Values)
        {
            Assert.Equal(idx * 2, value);
            count++;
            idx++;
        }
        Assert.Equal(500, count);
    }

    [Fact]
    public void NativeArray_Test()
    {
        using var array = new NativeArray<int>(1_000);

        for (int i = 0; i < 1_000; i++)
        {
            array.Set(i, i);
        }

        for (int i = 0; i < 1_000; i++)
        {
            Assert.Equal(i, array.Get(i));
        }
    }
}