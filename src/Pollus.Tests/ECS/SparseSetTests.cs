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
}