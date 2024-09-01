namespace Pollus.Engine.Physics;

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

[StructLayout(LayoutKind.Explicit)]
public struct CollisionShape : IComponent
{
    [FieldOffset(0)] public readonly CollisionShapeType Type;
    [FieldOffset(4)] Circle2D Circle;
    [FieldOffset(4)] Bounds2D Rectangle;

    public Intersection GetIntersection(in Transform2 selfTransform, in CollisionShape otherShape, in Transform2 otherTransform)
    {   
        return Type switch
        {
            CollisionShapeType.Circle => otherShape.Type switch {
                CollisionShapeType.Circle => Circle.Translate(selfTransform.Position).GetIntersection(otherShape.Circle.Translate(otherTransform.Position)),
                CollisionShapeType.Rectangle => Circle.Translate(selfTransform.Position).GetIntersection(otherShape.Rectangle.Translate(otherTransform.Position)),
                _ => throw new System.NotImplementedException(),
            },
            CollisionShapeType.Rectangle => otherShape.Type switch {
                CollisionShapeType.Circle => Rectangle.Translate(selfTransform.Position).GetIntersection(otherShape.Circle.Translate(otherTransform.Position)),
                CollisionShapeType.Rectangle => Rectangle.Translate(selfTransform.Position).GetIntersection(otherShape.Rectangle.Translate(otherTransform.Position)),
                _ => throw new System.NotImplementedException(),
            },
            _ => throw new System.NotImplementedException(),
        };
    }
}

public struct CircleShape2D : ComponentWrapper<CircleShape2D>.Target<CollisionShape>
{
    static CircleShape2D() => ComponentWrapper<CircleShape2D>.Target<CollisionShape>.Init();

    public readonly CollisionShapeType Type = CollisionShapeType.Circle;
    public required Circle2D Shape;

    public CircleShape2D() { }
}

public struct RectangleShape2D : ComponentWrapper<RectangleShape2D>.Target<CollisionShape>
{
    static RectangleShape2D() => ComponentWrapper<RectangleShape2D>.Target<CollisionShape>.Init();

    public readonly CollisionShapeType Type = CollisionShapeType.Rectangle;
    public required Bounds2D Shape;

    public RectangleShape2D() { }
}