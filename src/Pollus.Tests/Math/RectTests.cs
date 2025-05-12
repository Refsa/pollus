namespace Pollus.Tests.Math;

using Pollus.Mathematics;

public class RectTests
{
    [Fact]
    public void MoveRect()
    {
        var rect = new Rect(0, 0, 10, 10);
        rect.Move(new Vec2f(1, 1));
        Assert.Equal(new Vec2f(1, 1), rect.Min);
        Assert.Equal(new Vec2f(11, 11), rect.Max);
    }

    [Fact]
    public void ScaleRect()
    {
        var rect = new Rect(0, 0, 10, 10);
        rect.Scale(new Vec2f(2, 2));
        Assert.Equal(new Vec2f(0, 0), rect.Min);
        Assert.Equal(new Vec2f(20, 20), rect.Max);
    }

    [Fact]
    public void ContainsPoint()
    {
        var rect = new Rect(0, 0, 10, 10);
        Assert.True(rect.Contains(new Vec2f(5, 5)));
        Assert.False(rect.Contains(new Vec2f(15, 15)));
    }

    [Fact]
    public void ContainsRect()
    {
        var rect = new Rect(0, 0, 10, 10);
        Assert.True(rect.Contains(new Rect(0, 0, 5, 5)));
        Assert.False(rect.Contains(new Rect(15, 15, 20, 20)));
    }

    [Fact]
    public void IntersectsRect()
    {
        var rect = new Rect(0, 0, 10, 10);
        Assert.True(rect.Intersects(new Rect(5, 5, 15, 15)));
        Assert.False(rect.Intersects(new Rect(15, 15, 20, 20)));
    }

    [Fact]
    public void IntersectionPoint()
    {
        var rect = new Rect(0, 0, 10, 10);
        Assert.Equal(new Vec2f(5, 5), rect.IntersectionPoint(new Rect(5, 5, 15, 15)));
    }

    [Fact]
    public void Extents()
    {
        var rect = new Rect(0, 0, 10, 10);
        Assert.Equal(new Vec2f(5, 5), rect.Extents());
    }

    [Fact]
    public void Center()
    {
        var rect = new Rect(0, 0, 10, 10);
        Assert.Equal(new Vec2f(5, 5), rect.Center());
    }
}
