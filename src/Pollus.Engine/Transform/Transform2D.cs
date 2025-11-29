namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Engine.Reflect;
using Pollus.Engine.Tween;
using Pollus.Mathematics;

[Reflect]
public partial struct Transform2D : ITransform, IComponent
{
    public static EntityBuilder<Transform2D, GlobalTransform> Bundle => new(
        Default,
        GlobalTransform.Default
    );

    public static readonly Transform2D Default = new()
    {
        Position = Vec2f.Zero,
        Scale = Vec2f.One,
        Rotation = 0f
    };

    public static Transform2D Create(Vec2f position, Vec2f scale, float rotation)
    {
        return new()
        {
            Position = position,
            Scale = scale,
            Rotation = rotation
        };
    }

    public Vec2f Position;
    public Vec2f Scale;
    public float Rotation;

    public Vec2f Left => new(MathF.Cos(Rotation - MathF.PI * 0.5f), MathF.Sin(Rotation - MathF.PI * 0.5f));
    public Vec2f Right => new(MathF.Cos(Rotation + MathF.PI * 0.5f), MathF.Sin(Rotation + MathF.PI * 0.5f));
    public Vec2f Up => new(MathF.Cos(Rotation), MathF.Sin(Rotation));
    public Vec2f Down => new(MathF.Cos(Rotation + MathF.PI), MathF.Sin(Rotation + MathF.PI));

    public Mat4f ToMat4f()
    {
        return Mat4f.FromTRS(Position, Rotation.Radians(), Scale);
    }

    public Mat4f ToMat4f_Row()
    {
        return Mat4f.FromTRS_Row(Position, Rotation.Radians(), Scale);
    }

    public GlobalTransform ToGlobalTransform(Mat4f parentTransform)
    {
        return new GlobalTransform
        {
            Value = ToMat4f() * parentTransform,
        };
    }
}
