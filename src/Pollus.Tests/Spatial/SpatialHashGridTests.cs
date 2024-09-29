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

        var query = new ArrayList<SpatialHashGrid<int>.CellEntry>();

        {
            grid.Query(new Vec2f(0, 0), 1f, 1 << 0, query);
            Assert.Equal(1, query.Count);
            Assert.Equal(1, query.AsSpan()[0].Data);
        }
        query.Clear();

        {
            grid.Query(new Vec2f(2, 0), 1f, 1 << 0, query);
            Assert.Equal(0, query.Count);
        }
        query.Clear();

        {
            grid.Query(new Vec2f(2, 0), 2f, 1 << 0, query);
            Assert.Equal(1, query.Count);
            Assert.Equal(1, query.AsSpan()[0].Data);
        }
    }

    [Fact]
    public void TestInsertAndQuery_DifferentLayer()
    {
        var grid = new SpatialHashGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(0, 0), 1f, 1 << 0);

        var query = new ArrayList<SpatialHashGrid<int>.CellEntry>();

        grid.Query(new Vec2f(0, 0), 1f, 1 << 1, query);
        Assert.Equal(0, query.Count);
    }

    [Fact]
    public void TestInsertAndQuery_DifferentCell()
    {
        var grid = new SpatialHashGrid<int>(10, 100, 100);

        grid.Insert(1, new Vec2f(0, 0), 1f, 1 << 0);

        var query = new ArrayList<SpatialHashGrid<int>.CellEntry>();

        grid.Query(new Vec2f(15, 0), 1f, 1 << 0, query);
        Assert.Equal(0, query.Count);
        query.Clear();

        grid.Query(new Vec2f(15, 0), 20f, 1 << 0, query);
        Assert.Equal(1, query.Count);
        Assert.Equal(1, query.AsSpan()[0].Data);
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

        var query = new ArrayList<SpatialHashGrid<int>.CellEntry>();
        grid.Query(new Vec2f(5f, 5f), 7.1f, 1 << 0, query);
        Assert.Equal(10_000, query.Count);
    }
}
