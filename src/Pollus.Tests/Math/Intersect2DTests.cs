namespace Pollus.Tests.Mathematics;

using Pollus.Mathematics;
using Pollus.Mathematics.Collision2D;

public class Intersect2DTests
{
    [Fact]
    public void Line_ClosestPoint()
    {
        Line2D line = new Line2D(new Vec2f(0, 0), new Vec2f(10, 0));
        {
            Vec2f point = new Vec2f(0, 1);
            var closest = line.ClosestPoint(point);
            Assert.Equal(new Vec2f(0, 0), closest);
        }
        {
            Vec2f point = new Vec2f(10, 1);
            var closest = line.ClosestPoint(point);
            Assert.Equal(new Vec2f(10, 0), closest);
        }
        {
            Vec2f point = new Vec2f(5, 5);
            var closest = line.ClosestPoint(point);
            Assert.Equal(new Vec2f(5, 0), closest);
        }
        {
            Vec2f point = new Vec2f(5, -5);
            var closest = line.ClosestPoint(point);
            Assert.Equal(new Vec2f(5, 0), closest);
        }
        {
            Vec2f point = new Vec2f(5, 0);
            var closest = line.ClosestPoint(point);
            Assert.Equal(new Vec2f(5, 0), closest);
        }
    }

    [Fact]
    public void Circle_ClosestPoint()
    {
        Circle2D circle = new Circle2D(Vec2f.Zero, 5f);
        {
            Vec2f point = new Vec2f(0, 10);
            var closest = circle.ClosestPoint(point);
            Assert.Equal(new Vec2f(0, 5), closest);
        }
        {
            Vec2f point = new Vec2f(0, -10);
            var closest = circle.ClosestPoint(point);
            Assert.Equal(new Vec2f(0, -5), closest);
        }
        {
            Vec2f point = new Vec2f(5, 0);
            var closest = circle.ClosestPoint(point);
            Assert.Equal(new Vec2f(5, 0), closest);
        }
        {
            Vec2f point = new Vec2f(-5, 0);
            var closest = circle.ClosestPoint(point);
            Assert.Equal(new Vec2f(-5, 0), closest);
        }
        {
            Vec2f point = new Vec2f(3, 4);
            var closest = circle.ClosestPoint(point);
            Assert.Equal(new Vec2f(3, 4), closest);
        }
        {
            Vec2f point = new Vec2f(3, 6);
            var closest = circle.ClosestPoint(point);
            Assert.Equal(point.Normalized() * circle.Radius, closest);
        }
    }

    [Fact]
    public void Bounds_ClosestPoint()
    {
        Bounds2D bounds = new Bounds2D(Vec2f.One * -5f, Vec2f.One * 5f);
        {
            Vec2f point = new Vec2f(0, 10);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(0, 5), closest);
        }
        {
            Vec2f point = new Vec2f(0, -10);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(0, -5), closest);
        }
        {
            Vec2f point = new Vec2f(5, 0);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(5, 0), closest);
        }
        {
            Vec2f point = new Vec2f(-5, 0);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(-5, 0), closest);
        }
        {
            Vec2f point = new Vec2f(3, 4);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(3, 4), closest);
        }
        {
            Vec2f point = new Vec2f(3, 6);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(3, 5), closest);
        }
        {
            Vec2f point = new Vec2f(10f, 10f);
            var closest = bounds.ClosestPoint(point);
            Assert.Equal(new Vec2f(5, 5), closest);
        }
    }

    [Fact]
    public void Line_Inside()
    {
        Line2D line = new Line2D(new Vec2f(0, 0), new Vec2f(10, 0));
        {
            Vec2f point = new Vec2f(5, 0);
            Assert.True(line.Inside(point));
        }
        {
            Vec2f point = new Vec2f(5, 5);
            Assert.False(line.Inside(point));
        }
        {
            Vec2f point = new Vec2f(5, -5);
            Assert.False(line.Inside(point));
        }
    }

    [Fact]
    public void Circle_Inside()
    {
        Circle2D circle = new Circle2D(Vec2f.Zero, 5f);
        {
            Vec2f point = new Vec2f(0, 5);
            Assert.True(circle.Inside(point));
        }
        {
            Vec2f point = new Vec2f(0, 6);
            Assert.False(circle.Inside(point));
        }
        {
            Vec2f point = new Vec2f(0, -5);
            Assert.True(circle.Inside(point));
        }
        {
            Vec2f point = new Vec2f(0, -6);
            Assert.False(circle.Inside(point));
        }
    }

    [Fact]
    public void Bounds_Inside()
    {
        Bounds2D bounds = new Bounds2D(Vec2f.One * -5f, Vec2f.One * 5f);
        {
            Vec2f point = new Vec2f(0, 0);
            Assert.True(bounds.Inside(point));
        }
        {
            Vec2f point = new Vec2f(5, 5);
            Assert.True(bounds.Inside(point));
        }
        {
            Vec2f point = new Vec2f(5, 6);
            Assert.False(bounds.Inside(point));
        }
        {
            Vec2f point = new Vec2f(5, -6);
            Assert.False(bounds.Inside(point));
        }
    }

    [Fact]
    public void Ray_Ray_Intersection()
    {
        Ray2D ray1 = new Ray2D(new Vec2f(0, 0), new Vec2f(1, 1));
        Ray2D ray2 = new Ray2D(new Vec2f(0, 1), new Vec2f(1, -1));

        var intersection = ray1.GetIntersection(ray2);

        AssertIntersection(intersection, new Vec2f(0.5f, 0.5f), new Vec2f(0.7071067f, -0.7071068f), 0.70710677f);
    }

    [Fact]
    public void Ray_Ray_No_Intersection_Parallel()
    {
        Ray2D ray1 = new Ray2D(new Vec2f(0, 0), new Vec2f(1, 1));
        Ray2D ray2 = new Ray2D(new Vec2f(0, 1), new Vec2f(1, 1));

        var intersection = ray1.GetIntersection(ray2);

        Assert.False(intersection.IsIntersecting);
    }

    [Fact]
    public void Ray_Bounds_Intersection_Left()
    {
        Ray2D ray = new Ray2D(-Vec2f.Right, Vec2f.Right);
        Bounds2D bounds = new Bounds2D(new Vec2f(-0.5f, -0.5f), new Vec2f(0.5f, 0.5f));

        var intersection = ray.GetIntersection(bounds);

        AssertIntersection(intersection, new Vec2f(-0.5f, 0), Vec2f.Left, 0.5f);
    }

    [Fact]
    public void Ray_Bounds_Intersection_Right()
    {
        Ray2D ray = new Ray2D(-Vec2f.Left, Vec2f.Left);
        Bounds2D bounds = new Bounds2D(new Vec2f(-0.5f, -0.5f), new Vec2f(0.5f, 0.5f));

        var intersection = ray.GetIntersection(bounds);

        AssertIntersection(intersection, new Vec2f(0.5f, 0), Vec2f.Right, 0.5f);
    }

    [Fact]
    public void Ray_Bounds_Intersection_Up()
    {
        Ray2D ray = new Ray2D(-Vec2f.Down, Vec2f.Down);
        Bounds2D bounds = new Bounds2D(new Vec2f(-0.5f, -0.5f), new Vec2f(0.5f, 0.5f));

        var intersection = ray.GetIntersection(bounds);

        AssertIntersection(intersection, new Vec2f(0, 0.5f), Vec2f.Up, 0.5f);
    }

    [Fact]
    public void Ray_Bounds_Intersection_Down()
    {
        Ray2D ray = new Ray2D(-Vec2f.Up, Vec2f.Up);
        Bounds2D bounds = new Bounds2D(new Vec2f(-0.5f, -0.5f), new Vec2f(0.5f, 0.5f));

        var intersection = ray.GetIntersection(bounds);

        AssertIntersection(intersection, new Vec2f(0, -0.5f), Vec2f.Down, 0.5f);
    }

    [Fact]
    public void Ray_Bounds_Intersection_Corners()
    {
        Ray2D ray = new Ray2D(new Vec2f(-1, -1), (Vec2f.Right + Vec2f.Up).Normalized());
        Bounds2D bounds = new Bounds2D(new Vec2f(0, 0), new Vec2f(1, 1));

        var intersection = ray.GetIntersection(bounds);

        AssertIntersection(intersection, new Vec2f(0, 0), -ray.Direction, new Vec2f(1f, 1f).Length());
    }

    [Fact]
    public void Bounds_Bounds_Intersection_East()
    {
        Bounds2D center = new Bounds2D(new Vec2f(0, 0), new Vec2f(2, 1));
        Bounds2D leftBounds = new Bounds2D(new Vec2f(-1, 0), new Vec2f(1, 1));

        var intersection = center.GetIntersection(leftBounds);

        AssertIntersection(intersection, intersection.Point, Vec2f.Left, 0.5f);
    }

    [Fact]
    public void Bounds_Bounds_Intersection_West()
    {
        Bounds2D center = new Bounds2D(new Vec2f(0, 0), new Vec2f(2, 1));
        Bounds2D rightBounds = new Bounds2D(new Vec2f(1, 0), new Vec2f(3, 1));

        var intersection = center.GetIntersection(rightBounds);

        AssertIntersection(intersection, intersection.Point, Vec2f.Right, 0.5f);
    }

    [Fact]
    public void Bounds_Bounds_Intersection_North()
    {
        Bounds2D center = new Bounds2D(new Vec2f(0, 0), new Vec2f(1, 2));
        Bounds2D bottomBounds = new Bounds2D(new Vec2f(0, -1), new Vec2f(1, 1));

        var intersection = center.GetIntersection(bottomBounds);

        AssertIntersection(intersection, intersection.Point, Vec2f.Down, 0.5f);
    }

    [Fact]
    public void Bounds_Bounds_Intersection_South()
    {
        Bounds2D center = new Bounds2D(new Vec2f(0, 0), new Vec2f(1, 2));
        Bounds2D topBounds = new Bounds2D(new Vec2f(0, 1), new Vec2f(1, 3));

        var intersection = center.GetIntersection(topBounds);

        AssertIntersection(intersection, intersection.Point, Vec2f.Up, 0.5f);
    }

    [Fact]
    public void Bounds_Bounds_No_Overlap()
    {
        Bounds2D center = new Bounds2D(new Vec2f(0, 0), new Vec2f(1, 1));
        Bounds2D rightBounds = new Bounds2D(new Vec2f(2, 0), new Vec2f(3, 1));

        var intersection = center.GetIntersection(rightBounds);

        Assert.False(intersection.IsIntersecting);
    }

    [Fact]
    public void Circle_Circle_Intersection()
    {
        Circle2D circle1 = new Circle2D(new Vec2f(0f, 0f), 0.5f);
        Circle2D circle2 = new Circle2D(new Vec2f(0.5f, 0f), 0.5f);

        var intersection = circle1.GetIntersection(circle2);

        AssertIntersection(intersection, new Vec2f(0.5f, 0), Vec2f.Right, -0.5f);
    }

    [Fact]
    public void Circle_Circle_Intersection_Boundary()
    {
        Circle2D circle1 = new Circle2D(new Vec2f(0f, 0f), 0.5f);
        Circle2D circle2 = new Circle2D(new Vec2f(1f, 0f), 0.5f);

        var intersection = circle1.GetIntersection(circle2);

        AssertIntersection(intersection, new Vec2f(0.5f, 0), Vec2f.Right, 0f);
    }

    [Fact]
    public void Circle_Ray_Intersection()
    {
        Circle2D circle = new Circle2D(new Vec2f(0, 0), 1);
        Ray2D ray = new Ray2D(new Vec2f(-2, 0), Vec2f.Right);

        var intersection = circle.GetIntersection(ray);

        AssertIntersection(intersection, new Vec2f(-1, 0), Vec2f.Left, 1);
    }

    [Fact]
    public void Circle_Ray_Intersection_Grace()
    {
        Circle2D circle = new Circle2D(new Vec2f(0f, 0f), 1f);
        Ray2D ray = new Ray2D(new Vec2f(-1f, 1f), Vec2f.Right);

        var intersection = circle.GetIntersection(ray);

        AssertIntersection(intersection, new Vec2f(0, 1), Vec2f.Up, 1);
    }

    [Fact]
    public void Circle_Bounds_Intersection()
    {
        Circle2D circle = new Circle2D(new Vec2f(-1, 0), 1);
        Bounds2D bounds = new Bounds2D(new Vec2f(0, -2), new Vec2f(2, 2));

        var intersection = circle.GetIntersection(bounds);

        AssertIntersection(intersection, new Vec2f(0, 0), Vec2f.Left, 0);
    }

    [Fact]
    public void Line_Circle_Intersection()
    {
        Line2D line = new Line2D(new Vec2f(-10, 0), new Vec2f(10, 0));
        Circle2D circle = new Circle2D(new Vec2f(0, 0), 5);

        var intersection = line.GetIntersection(circle);

        AssertIntersection(intersection, Vec2f.Zero, Vec2f.Zero, 0);
    }

    [Fact]
    public void Line_Circle_No_Intersection()
    {
        Line2D line = new Line2D(new Vec2f(-10, 0), new Vec2f(-6, 0));
        Circle2D circle = new Circle2D(new Vec2f(0, 0), 5f);

        var intersection = line.GetIntersection(circle);

        Assert.False(intersection.IsIntersecting);
    }

    [Fact]
    public void Line_Line_Intersection()
    {
        Line2D line1 = new Line2D(new Vec2f(0, 0), new Vec2f(10, 0));
        Line2D line2 = new Line2D(new Vec2f(5, 5), new Vec2f(5, -5));

        var intersection = line1.GetIntersection(line2);

        AssertIntersection(intersection, new Vec2f(5, 0), Vec2f.One.Normalized() * new Vec2f(-1, 1), 0);
    }

    [Fact]
    public void Line_Line_No_Intersection()
    {
        Line2D line1 = new Line2D(new Vec2f(0, 0), new Vec2f(10, 0));
        Line2D line2 = new Line2D(new Vec2f(5, 5), new Vec2f(5, 6));

        var intersection = line1.GetIntersection(line2);

        Assert.False(intersection.IsIntersecting);
    }

    [Fact]
    public void Line_Line_Parallel()
    {
        Line2D line1 = new Line2D(new Vec2f(0, 0), new Vec2f(10, 0));
        Line2D line2 = new Line2D(new Vec2f(0, 1), new Vec2f(10, 1));

        var intersection = line1.GetIntersection(line2);

        Assert.False(intersection.IsIntersecting);
    }

    [Fact]
    public void Line_Bounds_Intersection()
    {
        Line2D line = new Line2D(new Vec2f(-10, 0), new Vec2f(10, 0));
        Bounds2D bounds = new Bounds2D(new Vec2f(-5, -5), new Vec2f(5, 5));

        var intersection = line.GetIntersection(bounds);

        AssertIntersection(intersection, Vec2f.Zero, Vec2f.Zero, 0);
    }

    [Fact]
    public void Line_Bounds_No_Intersection()
    {
        Line2D line = new Line2D(new Vec2f(0, 0), new Vec2f(10, 0));
        Bounds2D bounds = new Bounds2D(new Vec2f(5, 5), new Vec2f(15, 15));

        var intersection = line.GetIntersection(bounds);

        Assert.False(intersection.IsIntersecting);
    }

    static void AssertIntersection(Intersection2D intersection, Vec2f point, Vec2f normal, float distance)
    {
        Assert.True(intersection.IsIntersecting, "Intersection is not intersecting");
        Assert.Equal(point, intersection.Point, (a, b) => a.Approximately(b, 0.0001f));
        Assert.Equal(normal, intersection.Normal, (a, b) => a.Approximately(b, 0.0001f));
        Assert.Equal(distance, intersection.Distance, 4);
    }
}