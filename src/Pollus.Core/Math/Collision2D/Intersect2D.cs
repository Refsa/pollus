namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public struct Intersection2D
{
    public static readonly Intersection2D None = new() { IsIntersecting = false };

    public required bool IsIntersecting;
    public Vec2f Point;
    public Vec2f Normal;
    /// <summary>
    /// Penetration depth of the intersection.
    /// </summary>
    public float Distance;
}

public static partial class Intersect2D
{
    public static bool Intersects<TShapeA, TShapeB>(in TShapeA shapeA, in TShapeB shapeB)
        where TShapeA : struct, IShape2D
        where TShapeB : struct, IShape2D
    {
        return shapeA switch
        {
            Ray2D ray => shapeB switch
            {
                Ray2D other => ray.Intersects(other),
                Bounds2D bounds => ray.Intersects(bounds),
                Circle2D circle => ray.Intersects(circle),
                _ => throw new NotSupportedException($"Intersection between {shapeA.GetType().Name} and {shapeB.GetType().Name} is not supported.")
            },
            Bounds2D bounds => shapeB switch
            {
                Ray2D ray => bounds.Intersects(ray),
                Bounds2D other => bounds.Intersects(other),
                Circle2D circle => bounds.Intersects(circle),
                _ => throw new NotSupportedException($"Intersection between {shapeA.GetType().Name} and {shapeB.GetType().Name} is not supported.")
            },
            Circle2D circle => shapeB switch
            {
                Ray2D ray => circle.Intersects(ray),
                Bounds2D bounds => circle.Intersects(bounds),
                Circle2D other => circle.Intersects(other),
                _ => throw new NotSupportedException($"Intersection between {shapeA.GetType().Name} and {shapeB.GetType().Name} is not supported.")
            },
            _ => throw new NotSupportedException($"Intersection between {shapeA.GetType().Name} and {shapeB.GetType().Name} is not supported.")
        };
    }
}