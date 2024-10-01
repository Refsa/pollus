namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public record struct Bounds2D(Vec2f Min, Vec2f Max) : IShape2D
{
    public static Bounds2D FromCenterExtents(Vec2f center, Vec2f extents)
    {
        return new() { Min = center - extents, Max = center + extents };
    }

    public Bounds2D Translate(Vec2f translation) => new(Min + translation, Max + translation);

    public Bounds2D GetAABB()
    {
        return this;
    }
}

public static partial class Intersect2D
{
    public static bool Inside(this in Bounds2D bounds, in Vec2f point)
    {
        return point.X >= bounds.Min.X && point.X <= bounds.Max.X && point.Y >= bounds.Min.Y && point.Y <= bounds.Max.Y;
    }

    public static Vec2f ClosestPoint(this in Bounds2D bounds, in Vec2f point)
    {
        var closestX = Math.Clamp(point.X, bounds.Min.X, bounds.Max.X);
        var closestY = Math.Clamp(point.Y, bounds.Min.Y, bounds.Max.Y);
        return new Vec2f(closestX, closestY);
    }

    public static bool Intersects(this in Bounds2D bounds, in Bounds2D other)
    {
        return bounds.Min.X < other.Max.X && bounds.Max.X > other.Min.X && bounds.Min.Y < other.Max.Y && bounds.Max.Y > other.Min.Y;
    }

    public static bool Intersects(this in Bounds2D bounds, in Ray2D ray)
    {
        var invDir = new Vec2f(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y);

        float t1 = (bounds.Min.X - ray.Origin.X) * invDir.X;
        float t2 = (bounds.Max.X - ray.Origin.X) * invDir.X;
        float t3 = (bounds.Min.Y - ray.Origin.Y) * invDir.Y;
        float t4 = (bounds.Max.Y - ray.Origin.Y) * invDir.Y;

        float tmin = Math.Max(Math.Min(t1, t2), Math.Min(t3, t4));
        float tmax = Math.Min(Math.Max(t1, t2), Math.Max(t3, t4));

        return tmax >= 0 && tmin <= tmax;
    }

    public static bool Intersects(this in Bounds2D bounds, in Circle2D circle)
    {
        return circle.Intersects(bounds);
    }

    public static bool Intersects(this in Bounds2D bounds, in Line2D line)
    {
        if (bounds.Inside(line.Start) || bounds.Inside(line.End))
        {
            return true;
        }

        if (line.Intersects(new Line2D(bounds.Min, new Vec2f(bounds.Max.X, bounds.Min.Y))) ||
            line.Intersects(new Line2D(bounds.Min, new Vec2f(bounds.Min.X, bounds.Max.Y))) ||
            line.Intersects(new Line2D(bounds.Max, new Vec2f(bounds.Min.X, bounds.Max.Y))) ||
            line.Intersects(new Line2D(bounds.Max, new Vec2f(bounds.Max.X, bounds.Min.Y))))
        {
            return true;
        }

        return false;
    }

    public static Intersection2D GetIntersection<TShape>(this in Bounds2D bounds, in TShape other)
        where TShape : struct, IShape2D
    {
        return other switch
        {
            Circle2D otherCircle => GetIntersection(bounds, otherCircle),
            Bounds2D otherBounds => GetIntersection(bounds, otherBounds),
            Ray2D otherRay => GetIntersection(bounds, otherRay),
            Line2D otherLine => GetIntersection(bounds, otherLine),
            _ => throw new NotImplementedException(),
        };
    }

    public static Intersection2D GetIntersection(this in Bounds2D bounds, in Ray2D ray)
    {
        return ray.GetIntersection(bounds);
    }

    public static Intersection2D GetIntersection(this in Bounds2D bounds, in Circle2D circle)
    {
        return circle.GetIntersection(bounds);
    }

    public static Intersection2D GetIntersection(this in Bounds2D bounds, in Bounds2D other)
    {
        var x1 = Math.Max(bounds.Min.X, other.Min.X);
        var x2 = Math.Min(bounds.Max.X, other.Max.X);
        var y1 = Math.Max(bounds.Min.Y, other.Min.Y);
        var y2 = Math.Min(bounds.Max.Y, other.Max.Y);

        if (x1 >= x2 || y1 >= y2)
        {
            return new() { IsIntersecting = false };
        }

        var intersectionCenterX = (x1 + x2) / 2;
        var intersectionCenterY = (y1 + y2) / 2;

        var rectCenterX = (bounds.Min.X + bounds.Max.X) / 2;
        var rectCenterY = (bounds.Min.Y + bounds.Max.Y) / 2;

        var dx = intersectionCenterX - rectCenterX;
        var dy = intersectionCenterY - rectCenterY;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            return new()
            {
                IsIntersecting = true,
                Point = new Vec2f(x1, y1),
                Normal = dx < 0 ? Vec2f.Left : Vec2f.Right,
                Distance = Math.Abs(dx)
            };
        }
        else
        {
            return new()
            {
                IsIntersecting = true,
                Point = new Vec2f(x1, y1),
                Normal = dy < 0 ? Vec2f.Down : Vec2f.Up,
                Distance = Math.Abs(dy)
            };
        }
    }

    public static Intersection2D GetIntersection(this in Bounds2D bounds, in Line2D line)
    {
        return line.GetIntersection(bounds);
    }
}