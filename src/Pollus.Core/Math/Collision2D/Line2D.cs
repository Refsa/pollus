namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public record struct Line2D(Vec2f Start, Vec2f End) : IShape2D
{
    public Bounds2D GetAABB()
    {
        Vec2f min = new(Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y));
        Vec2f max = new(Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));
        return new() { Min = min, Max = max };
    }

    public Line2D Translate(Vec2f translation) => new(Start + translation, End + translation);
}

public static partial class Intersect2D
{
    /// <summary>
    /// Checks if point lies on the line
    /// </summary>
    public static bool Inside(this in Line2D line, in Vec2f point)
    {
        var lineDir = line.End - line.Start;
        var pointToStart = point - line.Start;
        var pointToEnd = point - line.End;

        return lineDir.Cross(pointToStart).Length() < float.Epsilon && pointToStart.Dot(pointToEnd) <= 0;
    }

    public static Vec2f ClosestPoint(this in Line2D line, Vec2f point)
    {
        var lineDir = line.End - line.Start;
        var pointToStart = point - line.Start;

        var lineLen = lineDir.Dot(lineDir);
        if (lineLen == 0) return line.Start;

        var u = lineDir.Dot(pointToStart) / lineLen;
        u = Math.Clamp(u, 0, 1);
        return line.Start + lineDir * u;
    }

    public static bool Intersects(this in Line2D line, in Line2D other)
    {
        var p1 = line.Start; var p2 = line.End;
        var p3 = other.Start; var p4 = other.End;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return false;

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }

    public static bool Intersects(this in Line2D line, in Ray2D ray)
    {
        var p1 = line.Start; var p2 = line.End;
        var p3 = ray.Origin; var p4 = ray.Origin + ray.Direction;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return false;

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        return ua >= 0 && ub >= 0 && ub <= 1;
    }

    public static bool Intersects(this in Line2D line, in Circle2D circle)
    {
        var closestPoint = line.ClosestPoint(circle.Center);
        return (circle.Center - closestPoint).Length() < circle.Radius;
    }

    public static bool Intersects(this in Line2D line, in Bounds2D bounds)
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

    public static Intersection2D GetIntersection(this in Line2D line, in Line2D other)
    {
        var p1 = line.Start; var p2 = line.End;
        var p3 = other.Start; var p4 = other.End;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return Intersection2D.None;

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1) return Intersection2D.None;

        var point = new Vec2f(p1.X + ua * (p2.X - p1.X), p1.Y + ua * (p2.Y - p1.Y));
        return new()
        {
            IsIntersecting = true,
            Point = point,
            Normal = (p1 - p2).Cross(p3 - p4).Normalized(),
            Distance = 0
        };
    }

    public static Intersection2D GetIntersection(this in Line2D line, in Circle2D circle)
    {
        var p1 = line.Start; var p2 = line.End;
        var center = circle.Center; var radius = circle.Radius;

        var d = p2 - p1;
        var f = p1 - center;

        float a = d.Dot(d);
        float b = 2 * f.Dot(d);
        float c = f.Dot(f) - radius * radius;

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0) return Intersection2D.None;

        discriminant = Math.Sqrt(discriminant);

        float t1 = (-b - discriminant) / (2 * a);
        float t2 = (-b + discriminant) / (2 * a);

        var pointA = Vec2f.Zero;
        var pointB = Vec2f.Zero;

        if (t2 >= 0 && t2 <= 1) pointA = p1 + d * t2;
        if (t1 >= 0 && t1 <= 1) pointB = p1 + d * t1;
        if (pointA == Vec2f.Zero && pointB == Vec2f.Zero) return Intersection2D.None;

        return new()
        {
            IsIntersecting = true,
        };
    }

    public static Intersection2D GetIntersection(this in Line2D line, in Bounds2D bounds)
    {
        float x1 = line.Start.X; float y1 = line.Start.Y;
        float x2 = line.End.X; float y2 = line.End.Y;

        float l = bounds.Min.X; float t = bounds.Min.Y;
        float r = bounds.Max.X; float b = bounds.Max.Y;

        // normalize segment
        float dx = x2 - x1; float dy = y2 - y1;
        float d = MathF.Sqrt(dx * dx + dy * dy);
        if (d == 0) return Intersection2D.None;
        float nx = dx / d; float ny = dy / d;

        // minimum and maximum intersection values
        float tmin = 0; float tmax = d;

        // x-axis check
        if (nx == 0)
        {
            if (x1 < l || x1 > r) return Intersection2D.None;
        }
        else
        {
            float t1 = (l - x1) / nx;
            float t2 = (r - x1) / nx;
            if (t1 > t2) Math.Swap(ref t1, ref t2);
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
            if (tmin > tmax) return Intersection2D.None;
        }

        // y-axis check
        if (ny == 0)
        {
            if (y1 < t || y1 > b) return Intersection2D.None;
        }
        else
        {
            float t1 = (t - y1) / ny;
            float t2 = (b - y1) / ny;
            if (t1 > t2) Math.Swap(ref t1, ref t2);
            tmin = Math.Max(tmin, t1);
            tmax = Math.Min(tmax, t2);
            if (tmin > tmax) return Intersection2D.None;
        }

        // one point
        Vec2f pointA = new Vec2f(x1 + nx * tmin, y1 + ny * tmin);
        if (tmin == tmax)
        {
            return new Intersection2D()
            {
                IsIntersecting = true,
                Point = pointA,
            };
        }

        // two points
        Vec2f pointB = new Vec2f(x1 + nx * tmax, y1 + ny * tmax);
        return new Intersection2D()
        {
            IsIntersecting = true,
        };
    }

    public static Intersection2D GetIntersection(this in Line2D line, in Ray2D ray)
    {
        var p1 = line.Start; var p2 = line.End;
        var p3 = ray.Origin; var p4 = ray.Origin + ray.Direction;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return Intersection2D.None;

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        if (ua < 0 || ub < 0 || ub > 1) return Intersection2D.None;

        var point = new Vec2f(p1.X + ua * (p2.X - p1.X), p1.Y + ua * (p2.Y - p1.Y));
        return new()
        {
            IsIntersecting = true,
            Point = point,
            Normal = ray.Direction.Cross(p2 - p1).Normalized(),
        };
    }
}