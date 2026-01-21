namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;

public partial struct ClipRect : IComponent
{
    public RectInt Rect;

    public static RectInt Intersect(RectInt a, RectInt b)
    {
        var minX = int.Max(a.Min.X, b.Min.X);
        var minY = int.Max(a.Min.Y, b.Min.Y);
        var maxX = int.Min(a.Max.X, b.Max.X);
        var maxY = int.Min(a.Max.Y, b.Max.Y);

        if (minX > maxX || minY > maxY)
        {
            return new RectInt(0, 0, 0, 0);
        }

        return new RectInt(minX, minY, maxX, maxY);
    }
}
