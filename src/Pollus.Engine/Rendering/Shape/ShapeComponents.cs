namespace Pollus.Engine.Rendering;

using ECS;
using Graphics;
using Mathematics;
using Transform;
using Utils;

public partial struct ShapeDraw : IComponent
{
    public static EntityBuilder<ShapeDraw, Transform2D, GlobalTransform> Bundle => new(
        new()
        {
            MaterialHandle = Handle<ShapeMaterial>.Null,
            ShapeHandle = Handle<Shape>.Null,
            Color = Color.WHITE,
            Offset = Vec2f.Zero,
        },
        Transform2D.Default,
        GlobalTransform.Default
    );

    public required Handle<ShapeMaterial> MaterialHandle;
    public required Handle<Shape> ShapeHandle;
    public required Color Color;
    public Vec2f Offset;
}