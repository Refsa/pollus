namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class SparseSetTests
{
    [Fact]
    public void SparseSet_Add()
    {
        var sparseSet = new SparseSet<int>(1024);
        sparseSet.Add(0);
        sparseSet.Add(64);
        sparseSet.Add(128);
        sparseSet.Add(192);

        Assert.Equal(4, sparseSet.Length);
        Assert.True(sparseSet.Contains(0));
        Assert.True(sparseSet.Contains(64));
        Assert.True(sparseSet.Contains(128));
        Assert.True(sparseSet.Contains(192));

        Assert.False(sparseSet.Contains(1));
    }

    [Fact]
    public void SparseSet_Remove()
    {
        var sparseSet = new SparseSet<int>(1024);
        sparseSet.Add(0);
        sparseSet.Add(64);
        sparseSet.Add(128);
        sparseSet.Add(192);

        sparseSet.Remove(64);
        sparseSet.Remove(128);

        Assert.Equal(2, sparseSet.Length);
        Assert.True(sparseSet.Contains(0));
        Assert.False(sparseSet.Contains(64));
        Assert.False(sparseSet.Contains(128));
        Assert.True(sparseSet.Contains(192));
    }

    [Fact]
    public void SparseSet_Enumerator()
    {
        var sparseSet = new SparseSet<int>(1024);
        sparseSet.Add(0);
        sparseSet.Add(64);
        sparseSet.Add(128);
        sparseSet.Add(192);

        int i = 0;
        foreach (var item in sparseSet)
        {
            Assert.Equal(i * 64, item);
            i++;
        }
    }

    [Fact]
    public void SparseSet_T_Add()
    {
        var sparseSet = new SparseSet<int, int>(1024);
        sparseSet.Add(0, 10);
        sparseSet.Add(64, 20);
        sparseSet.Add(128, 30);
        sparseSet.Add(192, 40);

        Assert.Equal(4, sparseSet.Length);
        Assert.True(sparseSet.Contains(0));
        Assert.True(sparseSet.Contains(64));
        Assert.True(sparseSet.Contains(128));
        Assert.True(sparseSet.Contains(192));

        Assert.False(sparseSet.Contains(1));
    }

    [Fact]
    public void SparseSet_T_Remove()
    {
        var sparseSet = new SparseSet<int, int>(1024);
        sparseSet.Add(0, 10);
        sparseSet.Add(64, 20);
        sparseSet.Add(128, 30);
        sparseSet.Add(192, 40);

        sparseSet.Remove(64);
        sparseSet.Remove(128);

        Assert.Equal(2, sparseSet.Length);
        Assert.True(sparseSet.Contains(0));
        Assert.False(sparseSet.Contains(64));
        Assert.False(sparseSet.Contains(128));
        Assert.True(sparseSet.Contains(192));
    }

    [Fact]
    public void SparseSet_T_Enumerator()
    {
        var sparseSet = new SparseSet<int, int>(1024);
        sparseSet.Add(0, 10);
        sparseSet.Add(64, 20);
        sparseSet.Add(128, 30);
        sparseSet.Add(192, 40);

        int i = 0;
        foreach (ref var value in sparseSet)
        {
            Assert.Equal((i + 1) * 10, value);
            i++;
        }
    }

    [Fact]
    public void SparseSet_UInt_Add()
    {
        var sparseSet = new SparseSet<uint>(1024);
        sparseSet.Add(0u);
        sparseSet.Add(64u);
        sparseSet.Add(128u);
        sparseSet.Add(192u);

        Assert.Equal(4, sparseSet.Length);
        Assert.True(sparseSet.Contains(0u));
        Assert.True(sparseSet.Contains(64u));
        Assert.True(sparseSet.Contains(128u));
        Assert.True(sparseSet.Contains(192u));

        Assert.False(sparseSet.Contains(1u));
    }

    [Fact]
    public void SparseSet_Remove_DestroysSwappedValue()
    {
        var set = new SparseSet<int, int>(32);
        set.Add(0, 100);
        set.Add(1, 200);
        set.Add(2, 300);

        set.Remove(0);

        Assert.True(set.Contains(1));
        Assert.True(set.Contains(2));
        Assert.False(set.Contains(0));

        Assert.Equal(200, set.Get(1));

        Assert.Equal(300, set.Get(2));
    }

    [Fact]
    public void SparseSet_Remove_MiddleElement_PreservesRemainingValues()
    {
        var set = new SparseSet<int, int>(32);
        set.Add(10, 1000);
        set.Add(20, 2000);
        set.Add(30, 3000);
        set.Add(40, 4000);

        set.Remove(20);

        Assert.Equal(3, set.Length);
        Assert.Equal(1000, set.Get(10));
        Assert.Equal(3000, set.Get(30));
        Assert.Equal(4000, set.Get(40));
    }

    [Fact]
    public void SparseSet_Remove_First_ThenIterate_ValuesIntact()
    {
        var set = new SparseSet<int, int>(32);
        set.Add(0, 10);
        set.Add(1, 20);
        set.Add(2, 30);

        set.Remove(0);

        var values = new List<int>();
        foreach (ref var value in set)
        {
            values.Add(value);
        }

        Assert.Equal(2, values.Count);
        Assert.Contains(20, values);
        Assert.Contains(30, values);
    }
}