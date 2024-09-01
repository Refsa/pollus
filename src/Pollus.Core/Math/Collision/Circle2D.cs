namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public record struct Circle2D(Vec2f Center, float Radius) : IShape2D
{
    public Bounds2D GetAABB()
    {
        Vec2f min = Center - new Vec2f(Radius, Radius);
        Vec2f max = Center + new Vec2f(Radius, Radius);
        return new() { Min = min, Max = max };
    }
}
