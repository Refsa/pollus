namespace Pollus.Tests.ECS;

using Pollus.ECS;

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
    public void NativeMap_NegativeKey()
    {
        /* using var map = new NativeMap<int, int>(0);

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
        Assert.Equal(30, map.Get(-3)); */
    }

    [Fact]
    public void NativeMap_Large()
    {
        using var map = new NativeMap<int, int>(0);
        for (int i = 0; i < 1_000_000; i++)
        {
            map.Add(i, i);
        }
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