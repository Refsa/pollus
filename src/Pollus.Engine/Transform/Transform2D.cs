namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

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
    public float ZIndex;

    public readonly Vec2f Left => new(MathF.Cos(Rotation - MathF.PI * 0.5f), MathF.Sin(Rotation - MathF.PI * 0.5f));
    public readonly Vec2f Right => new(MathF.Cos(Rotation + MathF.PI * 0.5f), MathF.Sin(Rotation + MathF.PI * 0.5f));
    public readonly Vec2f Up => new(MathF.Cos(Rotation), MathF.Sin(Rotation));
    public readonly Vec2f Down => new(MathF.Cos(Rotation + MathF.PI), MathF.Sin(Rotation + MathF.PI));

    public readonly Mat4f ToMat4f()
    {
        return Mat4f.FromTRS(new Vec3f(Position, ZIndex), Rotation.Radians(), Scale);
    }

    public readonly Mat4f ToMat4f_Row()
    {
        return Mat4f.FromTRS_Row(new Vec3f(Position, ZIndex), Rotation.Radians(), Scale);
    }

    public readonly GlobalTransform ToGlobalTransform(Mat4f parentTransform)
    {
        return new GlobalTransform
        {
            Value = ToMat4f() * parentTransform,
        };
    }

    public static Mat4f ToMat4f(in ITransform transform)
    {
        return transform.ToMat4f();
    }
}