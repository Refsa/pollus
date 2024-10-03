namespace Pollus.Engine.Physics;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.ECS;
using Pollus.Engine.Transform;
using Pollus.Mathematics;
using Pollus.Mathematics.Collision2D;

public enum CollisionShapeType
{
    Circle,
    Rectangle,
}

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 20)]
public struct CollisionShape : IComponent
{
    [FieldOffset(0)] public CollisionShapeType Type;
    [FieldOffset(4)] Circle2D circle;
    [FieldOffset(4)] Bounds2D rectangle;

    public Intersection2D GetIntersection(in Transform2D selfTransform, in CollisionShape otherShape, in Transform2D otherTransform)
    {
        return Type switch
        {
            CollisionShapeType.Circle => otherShape.Type switch
            {
                CollisionShapeType.Circle => circle.Translate(selfTransform.Position).GetIntersection(otherShape.circle.Translate(otherTransform.Position)),
                CollisionShapeType.Rectangle => circle.Translate(selfTransform.Position).GetIntersection(otherShape.rectangle.Translate(otherTransform.Position)),
                _ => throw new NotImplementedException(),
            },
            CollisionShapeType.Rectangle => otherShape.Type switch
            {
                CollisionShapeType.Circle => rectangle.Translate(selfTransform.Position).GetIntersection(otherShape.circle.Translate(otherTransform.Position)),
                CollisionShapeType.Rectangle => rectangle.Translate(selfTransform.Position).GetIntersection(otherShape.rectangle.Translate(otherTransform.Position)),
                _ => throw new NotImplementedException(),
            },
            _ => throw new NotImplementedException(),
        };
    }

    public Intersection2D GetIntersection<TShape>(in Transform2D selfTransform, in TShape otherShape, in Transform2D otherTransform)
        where TShape : struct, IShape2D
    {
        return Type switch
        {
            CollisionShapeType.Circle => circle.Translate(selfTransform.Position).GetIntersection(otherShape.Translate(otherTransform.Position)),
            CollisionShapeType.Rectangle => rectangle.Translate(selfTransform.Position).GetIntersection(otherShape.Translate(otherTransform.Position)),
            _ => throw new NotImplementedException(),
        };
    }

    public TShape GetShape<TShape>() where TShape : struct, IShape2D
    {
        return Type switch
        {
            CollisionShapeType.Circle => Unsafe.As<Circle2D, TShape>(ref circle),
            CollisionShapeType.Rectangle => Unsafe.As<Bounds2D, TShape>(ref rectangle),
            _ => throw new NotImplementedException(),
        };
    }

    public Bounds2D GetAABB(in Transform2D transform)
    {
        return Type switch
        {
            CollisionShapeType.Circle => circle.Translate(transform.Position).GetAABB(),
            CollisionShapeType.Rectangle => rectangle.Translate(transform.Position).GetAABB(),
            _ => throw new NotImplementedException(),
        };
    }

    public Circle2D GetBoundingCircle(in Transform2D transform)
    {
        return Type switch
        {
            CollisionShapeType.Circle => circle.Translate(transform.Position),
            CollisionShapeType.Rectangle => new Circle2D(
                transform.Position + rectangle.Center,
                (rectangle.Max - rectangle.Min).Length() * 0.5f
            ),
            _ => throw new NotImplementedException(),
        };
    }

    public static CollisionShape Circle(float radius) => new()
    {
        Type = CollisionShapeType.Circle,
        circle = new Circle2D(Vec2f.Zero, radius),
    };

    public static CollisionShape Rectangle(in Vec2f center, in Vec2f extents) => new()
    {
        Type = CollisionShapeType.Rectangle,
        rectangle = Bounds2D.FromCenterExtents(center, extents),
    };
}
