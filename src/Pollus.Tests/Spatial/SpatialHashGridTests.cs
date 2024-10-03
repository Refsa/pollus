namespace Pollus.Tests.Spatial;

using Pollus.Collections;
using Pollus.Mathematics;
using Pollus.Spatial;

public class SpatialHashGridTests
{
    [Fact]
    public void TestInsertAndQuery_SameCell()
    {
        var grid = new SpatialHashGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(0, 0), 1f, 1 << 0);

        Span<int> query = stackalloc int[1024];

        {
            var count = grid.Query(new Vec2f(0, 0), 1f, 1 << 0, query);
            Assert.Equal(1, count);
            Assert.Equal(1, query[0]);
        }
        query.Clear();

        {
            var count = grid.Query(new Vec2f(2, 0), 1f, 1 << 0, query);
            Assert.Equal(0, count);
        }
        query.Clear();

        {
            var count = grid.Query(new Vec2f(2, 0), 2f, 1 << 0, query);
            Assert.Equal(1, count);
            Assert.Equal(1, query[0]);
        }
    }

    [Fact]
    public void TestInsertAndQuery_DifferentLayer()
    {
        var grid = new SpatialHashGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(0, 0), 1f, 1 << 0);

        Span<int> query = stackalloc int[1024];

        var count = grid.Query(new Vec2f(0, 0), 1f, 1 << 1, query);
        Assert.Equal(0, count);
    }

    [Fact]
    public void TestInsertAndQuery_DifferentCell()
    {
        var grid = new SpatialHashGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(0, 0), 1f, 1 << 0);

        Span<int> query = stackalloc int[1024];

        var count = grid.Query(new Vec2f(15, 0), 1f, 1 << 0, query);
        Assert.Equal(0, count);
        query.Clear();

        count = grid.Query(new Vec2f(15, 0), 20f, 1 << 0, query);
        Assert.Equal(1, count);
        Assert.Equal(1, query[0]);
        query.Clear();
    }

    [Fact]
    public void TestInsertAndQuery_Many()
    {
        var grid = new SpatialHashGrid<int>(10, 100, 100);

        for (int x = 0; x < 100; x++)
            for (int y = 0; y < 100; y++)
            {
                grid.Insert(x + y * 100, new Vec2f(x / 10f, y / 10f), 1f, 1 << 0);
            }

        Span<int> query = stackalloc int[10_000];
        var count = grid.Query(new Vec2f(5f, 5f), 7.1f, 1 << 0, query);
        Assert.Equal(10_000, count);
    }
}
