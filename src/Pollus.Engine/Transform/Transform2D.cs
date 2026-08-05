namespace Pollus.Engine.Transform;

using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Tween;

[Tween]
public partial struct Transform2D : ITransform, IComponent, IDefault<Transform2D>
{
    public static EntityBuilder<Transform2D, GlobalTransform> Bundle => new(
        Default,
        GlobalTransform.Default
    );

    public static Transform2D Default { get; } = new()
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Mat4f ToMat4f()
    {
        return Mat4f.FromTRS(new Vec3f(Position, ZIndex), Rotation * (float)Math.DEG2RAD, Scale);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Mat4f ToMat4f_Row()
    {
        return Mat4f.FromTRS_Row(new Vec3f(Position, ZIndex), Rotation * (float)Math.DEG2RAD, Scale);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Write(ref Mat4f destination)
    {
        var (sin, cos) = Rotation == 0f ? (0f, 1f) : Math.SinCos(Rotation * (float)Math.DEG2RAD);

        destination.Col0.X = cos * Scale.X;
        destination.Col0.Y = sin * Scale.X;
        destination.Col1.X = -sin * Scale.Y;
        destination.Col1.Y = cos * Scale.Y;
        destination.Col2.Z = 1f;
        destination.Col3.X = Position.X;
        destination.Col3.Y = Position.Y;
        destination.Col3.Z = ZIndex;
        destination.Col3.W = 1f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly GlobalTransform ToGlobalTransform(Mat4f parentTransform)
    {
        return new GlobalTransform
        {
            Value = ToMat4f() * parentTransform,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mat4f ToMat4f(in Transform2D transform)
    {
        return transform.ToMat4f();
    }
}