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

    public Intersection2D GetIntersection(in Transform2 selfTransform, in CollisionShape otherShape, in Transform2 otherTransform)
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

    public TShape GetShape<TShape>() where TShape : struct, IShape2D
    {
        return Type switch
        {
            CollisionShapeType.Circle => Unsafe.As<Circle2D, TShape>(ref circle),
            CollisionShapeType.Rectangle => Unsafe.As<Bounds2D, TShape>(ref rectangle),
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
