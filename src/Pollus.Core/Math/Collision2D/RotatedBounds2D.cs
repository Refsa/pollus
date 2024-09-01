namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public record struct RotatedBounds2D(Vec2f Min, Vec2f Max, float Rotation) : IShape2D
{
    public Bounds2D GetAABB()
    {
        Span<Vec2f> corners = stackalloc Vec2f[]
        {
            new Vec2f(Min.X, Min.Y),
            new Vec2f(Max.X, Min.Y),
            new Vec2f(Max.X, Max.Y),
            new Vec2f(Min.X, Max.Y)
        };

        Vec2f center = (Min + Max) / 2;
        Vec2f size = Max - Min;
        Vec2f halfSize = size / 2;
        Vec2f rotatedHalfSize = halfSize.Rotate(Rotation);
        Vec2f rotatedMin = center - rotatedHalfSize;
        Vec2f rotatedMax = center + rotatedHalfSize;

        foreach (Vec2f corner in corners)
        {
            Vec2f rotatedCorner = corner.Rotate(Rotation, center);
            rotatedMin = Vec2f.Min(rotatedMin, rotatedCorner);
            rotatedMax = Vec2f.Max(rotatedMax, rotatedCorner);
        }

        return new() { Min = rotatedMin, Max = rotatedMax };
    }
}
