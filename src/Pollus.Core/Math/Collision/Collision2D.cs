namespace Pollus.Mathematics.Collision2D;

using Pollus.Mathematics;

public struct Intersection
{
    public static readonly Intersection None = new Intersection { IsIntersecting = false };

    public required bool IsIntersecting;
    public Vec2f Point;
    public Vec2f Normal;
    public float Distance;
}

public static class Intersect
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

    #region Ray2D
    public static bool Intersects(this in Ray2D ray, in Ray2D other)
    {
        var p1 = ray.Origin;
        var p2 = ray.Origin + ray.Direction;
        var p3 = other.Origin;
        var p4 = other.Origin + other.Direction;

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

    public static Intersection GetIntersection(this in Ray2D ray, in Ray2D other)
    {
        var p1 = ray.Origin;
        var p2 = ray.Origin + ray.Direction;
        var p3 = other.Origin;
        var p4 = other.Origin + other.Direction;

        float denominator = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);
        if (denominator == 0) return new Intersection { IsIntersecting = false };

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denominator;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denominator;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1) return new Intersection { IsIntersecting = false };

        var point = new Vec2f(p1.X + ua * (p2.X - p1.X), p1.Y + ua * (p2.Y - p1.Y));
        var normal = (p2 - p1).Normalized().Rotate(-float.Pi / 2);
        float distance = (point - p1).Length();

        return new Intersection { IsIntersecting = true, Point = point, Normal = normal, Distance = distance };
    }

    public static Intersection GetIntersection(this in Ray2D ray, in Bounds2D other)
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
            return new Intersection { IsIntersecting = false };
        }

        float t = tmin < 0 ? tmax : tmin;
        var point = ray.Origin + t * ray.Direction;
        var normal = new Vec2f(
            t == t1 ? -1 : (t == t2 ? 1 : 0),
            t == t3 ? -1 : (t == t4 ? 1 : 0)
        );

        return new Intersection
        {
            IsIntersecting = true,
            Point = point,
            Normal = normal.Normalized(),
            Distance = t
        };
    }

    public static Intersection GetIntersection(this in Ray2D ray, in Circle2D circle)
    {
        return circle.GetIntersection(ray);
    }
    #endregion

    #region Bounds2D
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

    public static Intersection GetIntersection(this in Bounds2D bounds, in Ray2D ray)
    {
        return ray.GetIntersection(bounds);
    }

    public static Intersection GetIntersection(this in Bounds2D bounds, in Circle2D circle)
    {
        return circle.GetIntersection(bounds);
    }

    public static Intersection GetIntersection(this in Bounds2D bounds, in Bounds2D other)
    {
        var x1 = Math.Max(bounds.Min.X, other.Min.X);
        var x2 = Math.Min(bounds.Max.X, other.Max.X);
        var y1 = Math.Max(bounds.Min.Y, other.Min.Y);
        var y2 = Math.Min(bounds.Max.Y, other.Max.Y);

        if (x1 >= x2 || y1 >= y2)
        {
            return new Intersection { IsIntersecting = false };
        }

        var intersectionCenterX = (x1 + x2) / 2;
        var intersectionCenterY = (y1 + y2) / 2;

        var rectCenterX = (bounds.Min.X + bounds.Max.X) / 2;
        var rectCenterY = (bounds.Min.Y + bounds.Max.Y) / 2;

        var dx = intersectionCenterX - rectCenterX;
        var dy = intersectionCenterY - rectCenterY;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            return new Intersection { IsIntersecting = true, Point = new Vec2f(x1, y1), Normal = dx < 0 ? Vec2f.Left : Vec2f.Right, Distance = Math.Abs(dx) };
        }
        else
        {
            return new Intersection { IsIntersecting = true, Point = new Vec2f(x1, y1), Normal = dy < 0 ? Vec2f.Down : Vec2f.Up, Distance = Math.Abs(dy) };
        }
    }
    #endregion

    #region Circle2D
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

    public static Intersection GetIntersection(this in Circle2D circle, in Circle2D other)
    {
        var direction = other.Center - circle.Center;
        var distance = direction.Length();
        var normal = direction.Normalized();

        if (distance <= circle.Radius + other.Radius)
        {
            var point = circle.Center + normal * circle.Radius;
            return new Intersection { IsIntersecting = true, Point = point, Normal = normal, Distance = distance - circle.Radius - other.Radius };
        }

        return new Intersection { IsIntersecting = false };
    }

    public static Intersection GetIntersection(this in Circle2D circle, in Bounds2D bounds)
    {
        var closestPoint = new Vec2f(
            Math.Max(bounds.Min.X, Math.Min(circle.Center.X, bounds.Max.X)),
            Math.Max(bounds.Min.Y, Math.Min(circle.Center.Y, bounds.Max.Y))
        );

        var direction = circle.Center - closestPoint;
        var distance = direction.Length();

        if (distance <= circle.Radius)
        {
            var normal = direction.Normalized();
            var point = circle.Center + -normal * circle.Radius;
            return new Intersection { IsIntersecting = true, Point = point, Normal = normal, Distance = distance };
        }

        return new Intersection { IsIntersecting = false };
    }

    public static Intersection GetIntersection(this in Circle2D circle, in Ray2D ray)
    {
        var direction = ray.Direction;
        var distance = ray.Origin - circle.Center;
        var a = direction.Dot(direction);
        var b = 2 * distance.Dot(direction);
        var c = distance.Dot(distance) - circle.Radius * circle.Radius;
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            return new Intersection { IsIntersecting = false };
        }

        discriminant = Math.Sqrt(discriminant);
        var t1 = (-b - discriminant) / (2 * a);
        var t2 = (-b + discriminant) / (2 * a);

        if (t1 >= 0)
        {
            var point = ray.Origin + t1 * ray.Direction;
            var normal = (point - circle.Center).Normalized();
            return new Intersection { IsIntersecting = true, Point = point, Normal = normal, Distance = t1 };
        }

        if (t2 >= 0)
        {
            var point = ray.Origin + t2 * ray.Direction;
            var normal = (point - circle.Center).Normalized();
            return new Intersection { IsIntersecting = true, Point = point, Normal = normal, Distance = t2 };
        }

        return new Intersection { IsIntersecting = false };
    }
    #endregion
}