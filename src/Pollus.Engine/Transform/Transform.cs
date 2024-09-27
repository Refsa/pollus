namespace Pollus.Engine.Transform;

using Pollus.ECS;
using Pollus.Mathematics;

public interface ITransform
{
    Mat4f ToMat4f();
    GlobalTransform ToGlobalTransform(Mat4f parentTransform);
}

public struct Transform2D : ITransform, IComponent
{
    public static EntityBuilder<Transform2D, GlobalTransform> Bundle => new(
        Transform2D.Default,
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

public struct GlobalTransform : IComponent
{
    public static readonly GlobalTransform Default = new()
    {
        Value = Mat4f.Identity(),
    };

    public Mat4f Value;

    public Transform2D ToTransform2D()
    {
        return new Transform2D
        {
            Position = Value.GetTranslation().XY,
            Rotation = Value.GetRotationZ(),
            Scale = Value.GetScale().XY,
        };
    }

    public Transform3D ToTransform3D()
    {
        return new Transform3D
        {
            Position = Value.GetTranslation(),
            Rotation = Value.GetRotation(),
            Scale = Value.GetScale(),
        };
    }
}