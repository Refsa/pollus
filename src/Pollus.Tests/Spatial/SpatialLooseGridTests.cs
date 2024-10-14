namespace Pollus.Tests.Spatial;

using Pollus.Collections;
using Pollus.Mathematics;
using Pollus.Spatial;

public class SpatialLooseGridTests
{
    [Fact]
    public void TestInsertAndQuery_SameCell()
    {
        var grid = new SpatialLooseGrid<int>(10, 100, 100);

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
        var grid = new SpatialLooseGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(0, 0), 1f, 1 << 0);

        Span<int> query = stackalloc int[1024];

        var count = grid.Query(new Vec2f(0, 0), 1f, 1 << 1, query);
        Assert.Equal(0, count);
    }

    [Fact]
    public void TestInsertAndQuery_DifferentCell()
    {
        var grid = new SpatialLooseGrid<int>(10, 100, 100);

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
        var grid = new SpatialLooseGrid<int>(10, 100, 100);

        for (int x = 0; x < 100; x++)
            for (int y = 0; y < 100; y++)
            {
                grid.Insert(x + y * 100, new Vec2f(x / 10f, y / 10f), 1f, 1 << 0);
            }

        Span<int> query = stackalloc int[10_000];
        var count = grid.Query(new Vec2f(5f, 5f), 7.1f, 1 << 0, query);
        Assert.Equal(10_000, count);
    }

    [Fact]
    public void TestInsertAndQuery_ManyMore()
    {
        var grid = new SpatialLooseGrid<int>(64, 256, 512);

        for (int x = 0; x < 32; x++)
            for (int y = 0; y < 32; y++)
                for (int z = 0; z < 128; z++)
                {
                    grid.Insert(x + y * 32 + z * 32 * 32, new Vec2f(x, y), 4, 1u << 0);
                }

        Span<int> result = stackalloc int[1024];
        var count = grid.Query(new Vec2f(16, 16), 128, 1u << 0, result);
        Assert.Equal(1024, count);
    }

    [Fact]
    public void TestQueryOverCellBoundary()
    {
        var grid = new SpatialLooseGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(50, 5), 100f, 1u << 0);

        Span<int> query = stackalloc int[1024];

        var count = grid.Query(new Vec2f(5, 5), 5f, 1u << 0, query);
        Assert.Equal(1, count);
    }
}
