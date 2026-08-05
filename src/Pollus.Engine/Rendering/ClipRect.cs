namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;

public partial struct ClipRect : IComponent
{
    public Rect<int> Rect;

    public static Rect<int> Intersect(Rect<int> a, Rect<int> b)
    {
        var minX = int.Max(a.Min.X, b.Min.X);
        var minY = int.Max(a.Min.Y, b.Min.Y);
        var maxX = int.Min(a.Max.X, b.Max.X);
        var maxY = int.Min(a.Max.Y, b.Max.Y);

        if (minX > maxX || minY > maxY)
        {
            return new Rect<int>(0, 0, 0, 0);
        }

        return new Rect<int>(minX, minY, maxX, maxY);
    }
}
