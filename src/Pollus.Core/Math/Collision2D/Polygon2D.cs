namespace Pollus.Mathematics.Collision2D;

public record struct Triangle2D(Vec2f A, Vec2f B, Vec2f C) : IShape2D
{
    public Bounds2D GetAABB()
    {
        Vec2f min = new(Math.Min(A.X, Math.Min(B.X, C.X)), Math.Min(A.Y, Math.Min(B.Y, C.Y)));
        Vec2f max = new(Math.Max(A.X, Math.Max(B.X, C.X)), Math.Max(A.Y, Math.Max(B.Y, C.Y)));
        return new() { Min = min, Max = max };
    }
}

public struct Polygon2D : IShape2D
{
    public Bounds2D GetAABB()
    {
        throw new NotImplementedException();
    }
}


public partial class Intersect2D
{

}