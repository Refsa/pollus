namespace Pollus.Tests.Mathematics;

using Pollus.Mathematics;
using Pollus.Mathematics.Collision2D;

public class Collision2DTests
{
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

        AssertIntersection(intersection, new Vec2f(0, 0), Vec2f.Left, 1);
    }

    static void AssertIntersection(Intersection intersection, Vec2f point, Vec2f normal, float distance)
    {
        Assert.True(intersection.IsIntersecting, "Intersection is not intersecting");
        Assert.Equal(point, intersection.Point, (a, b) => a.Approximately(b, 0.0001f));
        Assert.Equal(normal, intersection.Normal, (a, b) => a.Approximately(b, 0.0001f));
        Assert.Equal(distance, intersection.Distance, 4);
    }
}