namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public record struct Ray2D(Vec2f Origin, Vec2f Direction, float Magnitude = float.MaxValue) : IShape2D
{
    public Bounds2D GetAABB()
    {
        var minx = Math.Min(Origin.X, Origin.X + Direction.X * Magnitude);
        var miny = Math.Min(Origin.Y, Origin.Y + Direction.Y * Magnitude);
        var maxx = Math.Max(Origin.X, Origin.X + Direction.X * Magnitude);
        var maxy = Math.Max(Origin.Y, Origin.Y + Direction.Y * Magnitude);

        return new Bounds2D(new Vec2f(minx, miny), new Vec2f(maxx, maxy));
    }
}
