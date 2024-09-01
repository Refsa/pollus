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

public static partial class Intersect2D
{
    public static bool Intersects(this in Ray2D ray, in Ray2D other)
    {
        var p1 = ray.Origin; var p2 = ray.Origin + ray.Direction;
        var p3 = other.Origin; var p4 = other.Origin + other.Direction;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return false;

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }

    public static bool Intersects(this in Ray2D ray, in Bounds2D bounds)
    {
        return bounds.Intersects(ray);
    }

    public static bool Intersects(this in Ray2D ray, in Circle2D circle)
    {
        return circle.Intersects(ray);
    }

    public static bool Intersects(this in Ray2D ray, in Line2D line)
    {
        return line.Intersects(ray);
    }

    public static Intersection2D GetIntersection(this in Ray2D ray, in Ray2D other)
    {
        var p1 = ray.Origin; var p2 = ray.Origin + ray.Direction;
        var p3 = other.Origin; var p4 = other.Origin + other.Direction;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return new() { IsIntersecting = false };

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1) return new() { IsIntersecting = false };

        var point = new Vec2f(p1.X + ua * (p2.X - p1.X), p1.Y + ua * (p2.Y - p1.Y));
        var normal = (p2 - p1).Normalized().Rotate(-float.Pi / 2);
        float distance = (point - p1).Length();

        return new()
        {
            IsIntersecting = true,
            Point = point,
            Normal = normal,
            Distance = distance
        };
    }

    public static Intersection2D GetIntersection(this in Ray2D ray, in Bounds2D other)
    {
        var invDir = new Vec2f(1.0f / ray.Direction.X, 1.0f / ray.Direction.Y);

        float t1 = (other.Min.X - ray.Origin.X) * invDir.X;
        float t2 = (other.Max.X - ray.Origin.X) * invDir.X;
        float t3 = (other.Min.Y - ray.Origin.Y) * invDir.Y;
        float t4 = (other.Max.Y - ray.Origin.Y) * invDir.Y;

        float tmin = Math.Max(Math.Min(t1, t2), Math.Min(t3, t4));
        float tmax = Math.Min(Math.Max(t1, t2), Math.Max(t3, t4));

        if (tmax < 0 || tmin > tmax)
        {
            return new() { IsIntersecting = false };
        }

        float t = tmin < 0 ? tmax : tmin;
        var point = ray.Origin + t * ray.Direction;
        var normal = new Vec2f(
            t == t1 ? -1 : (t == t2 ? 1 : 0),
            t == t3 ? -1 : (t == t4 ? 1 : 0)
        );

        return new()
        {
            IsIntersecting = true,
            Point = point,
            Normal = normal.Normalized(),
            Distance = t
        };
    }

    public static Intersection2D GetIntersection(this in Ray2D ray, in Circle2D circle)
    {
        return circle.GetIntersection(ray);
    }

    public static Intersection2D GetIntersection(this in Ray2D ray, in Line2D line)
    {
        return line.GetIntersection(ray);
    }
}