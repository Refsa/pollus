namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Engine.Tween;
using Pollus.Mathematics;

[Tweenable]
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

    public Vec2f Position;
    public Vec2f Scale;
    public float Rotation;

    public Vec2f Left => new(MathF.Cos(Rotation - MathF.PI * 0.5f), MathF.Sin(Rotation - MathF.PI * 0.5f));
    public Vec2f Right => new(MathF.Cos(Rotation + MathF.PI * 0.5f), MathF.Sin(Rotation + MathF.PI * 0.5f));
    public Vec2f Up => new(MathF.Cos(Rotation), MathF.Sin(Rotation));
    public Vec2f Down => new(MathF.Cos(Rotation + MathF.PI), MathF.Sin(Rotation + MathF.PI));

    public Mat4f ToMat4f()
    {
        return Mat4f.FromTRS(
            new(Position, 0f),
            Quat.AxisAngle(Vec3f.Forward, Rotation.Radians()),
            new(Scale, 1f)
        );
    }

    public GlobalTransform ToGlobalTransform(Mat4f parentTransform)
    {
        return new GlobalTransform
        {
            Value = ToMat4f() * parentTransform,
        };
    }
}
