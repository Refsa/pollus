namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

public struct Transform3D : ITransform, IComponent
{
    public static readonly EntityBuilder<Transform3D, GlobalTransform> Bundle = Entity.With(Transform3D.Default, GlobalTransform.Default);

    public static readonly Transform3D Default = new()
    {
        Position = Vec3f.Zero,
        Scale = Vec3f.One,
        Rotation = Quat.Identity(),
    };

    public Vec3f Position;
    public Vec3f Scale;
    public Quat Rotation;

    public Vec3f Forward => Rotation * Vec3f.Forward;
    public Vec3f Back => Rotation * Vec3f.Backward;
    public Vec3f Left => Rotation * Vec3f.Left;
    public Vec3f Right => Rotation * Vec3f.Right;
    public Vec3f Up => Rotation * Vec3f.Up;
    public Vec3f Down => Rotation * Vec3f.Down;

    public Mat4f ToMat4f()
    {
        return Mat4f.FromTRS(Position, Rotation, Scale);
    }

    public GlobalTransform ToGlobalTransform(Mat4f parentTransform)
    {
        return new GlobalTransform
        {
            Value = ToMat4f() * parentTransform,
        };
    }
}
