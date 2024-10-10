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

    public Circle2D Translate(Vec2f translation) => new(Center + translation, Radius);
}

public static partial class Intersect2D
{
    public static bool Inside(this in Circle2D circle, in Vec2f point)
    {
        return (point - circle.Center).Length() <= circle.Radius;
    }

    public static Vec2f ClosestPoint(this in Circle2D circle, in Vec2f point)
    {
        var direction = point - circle.Center;
        var distance = direction.Length();
        return circle.Center + direction.Normalized() * Math.Min(distance, circle.Radius);
    }

    public static bool Intersects(this in Circle2D circle, in Circle2D other)
    {
        return (circle.Center - other.Center).Length() < circle.Radius + other.Radius;
    }

    public static bool Intersects(this in Circle2D circle, in Bounds2D bounds)
    {
        var closestPoint = new Vec2f(
            Math.Max(bounds.Min.X, Math.Min(circle.Center.X, bounds.Max.X)),
            Math.Max(bounds.Min.Y, Math.Min(circle.Center.Y, bounds.Max.Y))
        );

        return (circle.Center - closestPoint).Length() < circle.Radius;
    }

    public static bool Intersects(this in Circle2D circle, in Ray2D ray)
    {
        var direction = ray.Direction;
        var distance = ray.Origin - circle.Center;
        var a = direction.Dot(direction);
        var b = 2 * distance.Dot(direction);
        var c = distance.Dot(distance) - circle.Radius * circle.Radius;
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return false;
        }

        discriminant = Math.Sqrt(discriminant);
        var t1 = (-b - discriminant) / (2 * a);
        var t2 = (-b + discriminant) / (2 * a);

        return t1 >= 0 || t2 >= 0;
    }

    public static bool Intersects(this in Circle2D circle, in Line2D line)
    {
        var closestPoint = line.ClosestPoint(circle.Center);
        return (circle.Center - closestPoint).Length() < circle.Radius;
    }

    public static Intersection2D GetIntersection<TShape>(this in Circle2D circle, in TShape other)
        where TShape : struct, IShape2D
    {
        return other switch
        {
            Circle2D otherCircle => GetIntersection(circle, otherCircle),
            Bounds2D otherBounds => GetIntersection(circle, otherBounds),
            Ray2D otherRay => GetIntersection(circle, otherRay),
            Line2D otherLine => GetIntersection(circle, otherLine),
            _ => throw new NotImplementedException(),
        };
    }

    public static Intersection2D GetIntersection(this in Circle2D circle, in Circle2D other)
    {
        var direction = other.Center - circle.Center;
        var distance = direction.Length();
        var normal = direction.Normalized();

        if (distance <= circle.Radius + other.Radius)
        {
            var point = circle.Center + normal * circle.Radius;
            return new()
            {
                IsIntersecting = true,
                Point = point,
                Normal = normal,
                Distance = distance - circle.Radius - other.Radius,
            };
        }

        return new() { IsIntersecting = false };
    }

    public static Intersection2D GetIntersection(this in Circle2D circle, in Bounds2D bounds)
    {
        var closestPoint = new Vec2f(
            Math.Max(bounds.Min.X, Math.Min(circle.Center.X, bounds.Max.X)),
            Math.Max(bounds.Min.Y, Math.Min(circle.Center.Y, bounds.Max.Y))
        );

        var direction = circle.Center - closestPoint;
        var distance = direction.Length();

        if (distance <= circle.Radius)
        {
            Vec2f normal;
            Vec2f pointOnEdge;

            if (distance == 0)
            {
                var distanceToMin = circle.Center - bounds.Min;
                var distanceToMax = bounds.Max - circle.Center;

                if (distanceToMin.X < distanceToMin.Y && distanceToMin.X < distanceToMax.X && distanceToMin.X < distanceToMax.Y) normal = Vec2f.Left;
                else if (distanceToMax.X < distanceToMin.Y && distanceToMax.X < distanceToMax.Y) normal = Vec2f.Right;
                else if (distanceToMin.Y < distanceToMax.Y) normal = Vec2f.Down;
                else normal = Vec2f.Up;

                pointOnEdge = circle.Center + normal * circle.Radius;
            }
            else
            {
                normal = direction.Normalized();
                pointOnEdge = closestPoint + normal * (circle.Radius - distance);
            }

            return new()
            {
                IsIntersecting = true,
                Point = pointOnEdge,
                Normal = normal,
                Distance = circle.Radius - distance,
            };
        }

        return new() { IsIntersecting = false };
    }

    public static Intersection2D GetIntersection(this in Circle2D circle, in Ray2D ray)
    {
        var direction = ray.Direction;
        var distance = ray.Origin - circle.Center;
        var a = direction.Dot(direction);
        var b = 2 * distance.Dot(direction);
        var c = distance.Dot(distance) - circle.Radius * circle.Radius;
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return new() { IsIntersecting = false };
        }

        discriminant = Math.Sqrt(discriminant);
        var t1 = (-b - discriminant) / (2 * a);
        var t2 = (-b + discriminant) / (2 * a);

        if (t1 >= 0)
        {
            var point = ray.Origin + t1 * ray.Direction;
            var normal = (point - circle.Center).Normalized();
            return new()
            {
                IsIntersecting = true,
                Point = point,
                Normal = normal,
                Distance = t1,
            };
        }

        if (t2 >= 0)
        {
            var point = ray.Origin + t2 * ray.Direction;
            var normal = (point - circle.Center).Normalized();
            return new()
            {
                IsIntersecting = true,
                Point = point,
                Normal = normal,
                Distance = t2,
            };
        }

        return new() { IsIntersecting = false };
    }

    public static Intersection2D GetIntersection(this in Circle2D circle, in Line2D line)
    {
        var closestPoint = line.ClosestPoint(circle.Center);
        var direction = circle.Center - closestPoint;
        var distance = direction.Length();

        if (distance <= circle.Radius)
        {
            var normal = direction.Normalized();
            return new()
            {
                IsIntersecting = true,
                Point = closestPoint,
                Normal = normal,
                Distance = circle.Radius - distance,
            };
        }

        return new() { IsIntersecting = false };
    }
}